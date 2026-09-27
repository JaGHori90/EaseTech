# EaseTech

Ein Diplomprojekt aus dem Jahr 2025 – eine Plattform für technischen Support:
Kunden buchen Hilfe (z. B. PC, Smartphone, WLAN), Mitarbeiter bearbeiten Aufträge,
Rechnungen und Kontaktanfragen und bieten Video-Support an.

| Teil | Technologie | Ordner |
|---|---|---|
| Backend | ASP.NET Core 8 (Minimal APIs), EF Core, ASP.NET Identity, JWT | [`Api/`](Api) |
| Datenbank | PostgreSQL – gehostet auf [Neon](https://neon.tech) | – |
| Frontend | Angular 19, Bootstrap | [`Frontend/`](Frontend) |
| Video-Call | GetStream Video | – |
| Tests | MSTest + WebApplicationFactory (API), Jasmine/Karma (Angular), Playwright (E2E) | `Api/APITest`, `Frontend/src/**/*.spec.ts`, `Frontend/e2e` |

Die geplanten nächsten Schritte stehen in [`docs/ROADMAP.md`](docs/ROADMAP.md).

---

## Voraussetzungen

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js 22](https://nodejs.org) und [pnpm](https://pnpm.io) (`npm i -g pnpm`)
- Eine PostgreSQL-Datenbank – am einfachsten ein kostenloses Projekt auf **Neon**
  (alternativ lokal per Docker: `docker run -p 5432:5432 -e POSTGRES_PASSWORD=postgres postgres:16`)

## 1. Datenbank auf Neon anlegen

1. Auf <https://console.neon.tech> ein Projekt erstellen (Region z. B. *Frankfurt*).
2. Unter **Connect** den Connection String im Format **.NET** kopieren. Er sieht so aus:
   ```
   Host=ep-xxx-123456.eu-central-1.aws.neon.tech;Database=neondb;Username=neondb_owner;Password=***;SSL Mode=Require
   ```
3. Tipp: Für Entwicklung und Produktion zwei **Branches** in Neon verwenden (z. B. `dev` und `main`) –
   so testest du Migrationen, ohne echte Daten zu gefährden.

## 2. Secrets konfigurieren (niemals ins Git!)

Alle geheimen Werte stehen **nicht** in `appsettings.json`, sondern in den
[User-Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) (lokal)
bzw. in Umgebungsvariablen (Server).

```bash
cd Api/Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=...;Database=neondb;Username=...;Password=...;SSL Mode=Require"
dotnet user-secrets set "AppSettings:JWT_Secret" "$(openssl rand -base64 48)"
dotnet user-secrets set "GetStream:ApiKey" "<dein Stream API Key>"
dotnet user-secrets set "GetStream:SecretKey" "<dein Stream Secret>"

# Optional: ersten Admin automatisch anlegen
dotnet user-secrets set "Seed:AdminEmail" "<deine E-Mail>"
dotnet user-secrets set "Seed:AdminPassword" "<sicheres Passwort>"

# Optional (nur lokal): Passwort für die Demo-Benutzer
dotnet user-secrets set "Seed:DemoPassword" "<sicheres Passwort>"
```

Passwörter brauchen mindestens 8 Zeichen mit Groß- und Kleinbuchstaben, Ziffer und Sonderzeichen.

Auf einem Server heißen die Umgebungsvariablen gleich, nur mit `__` statt `:`,
z. B. `ConnectionStrings__DefaultConnection`, `AppSettings__JWT_Secret`.

| Schlüssel | Pflicht | Bedeutung |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | ✅ | PostgreSQL/Neon Connection String |
| `AppSettings:JWT_Secret` | ✅ | Schlüssel zum Signieren der Tokens, **mind. 32 Zeichen** – sonst startet die API nicht |
| `AppSettings:TokenLifetimeHours` | | Gültigkeit eines Logins (Standard 12 h) |
| `Cors:AllowedOrigins` | | Erlaubte Frontend-URLs (Standard `http://localhost:4200`) |
| `Database:MigrateOnStartup` | | Migrationen beim Start anwenden (in *Development* an) |
| `Seed:AdminEmail` / `Seed:AdminPassword` | | Legt beim Start einen Admin an, falls es ihn noch nicht gibt |
| `Seed:DemoData` | | Demo-Services und -Benutzer (in *Development* an) |
| `Seed:DemoPassword` | | Passwort der Demo-Benutzer – ohne diesen Wert werden keine Demo-Benutzer angelegt |
| `GetStream:ApiKey` / `GetStream:SecretKey` | für Video | Zugangsdaten von <https://getstream.io> |

## 3. Backend starten

```bash
cd Api/Api
dotnet run            # http://localhost:5001, Swagger: http://localhost:5001/swagger
```

Beim Start (Development) werden automatisch
- die Migrationen angewendet,
- die Rollen `Admin`, `Employee`, `Customer` angelegt,
- Demo-Services angelegt und – falls `Seed:DemoPassword` gesetzt ist – je ein Demo-Benutzer pro Rolle
  (`admin@easetech.local`, `employee@easetech.local`, `customer@easetech.local`) mit diesem Passwort.

> ⚠️ Demo-Daten nur lokal verwenden. In Produktion `Seed:DemoData` auf `false` lassen.

### Migrationen

```bash
dotnet tool install -g dotnet-ef --version 8.0.10
cd Api/Api
dotnet ef migrations add <Name>        # nach einer Änderung an den Models
dotnet ef database update              # nutzt ConnectionStrings__DefaultConnection
```

## 4. Frontend starten

```bash
cd Frontend
pnpm install
pnpm start            # http://localhost:4200
```

## 5. Tests

```bash
# Backend – startet die API im Speicher mit einer In-Memory-Datenbank
cd Api && dotnet test

# Frontend – Unit-Tests (einmalig, headless)
cd Frontend && pnpm run test:ci

# E2E (Backend + Frontend müssen laufen)
cd Frontend && npx playwright test
```

Bei jedem Push auf `develop` oder `main` laufen Build und Tests automatisch über
GitHub Actions ([`.github/workflows/ci.yml`](.github/workflows/ci.yml)).

## Branch-Strategie

- `develop` – laufende Entwicklung, alle Änderungen landen zuerst hier
- `main` – veröffentlichte, stabile Version (nur per Merge aus `develop`)
