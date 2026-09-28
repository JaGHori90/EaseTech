# EaseTech

**Einfacher Zugang zur Technologie** – eine Webplattform, die **ältere Menschen** und Personen mit wenig Technik-Erfahrung bei technischen Problemen unterstützt: vom WLAN-Problem am neuen Smart-TV über Druckereinstellungen bis zur unabhängigen Kauf- oder Reparaturberatung – online per **Video-Call** oder vor Ort.

Entstanden 2025 als **Diplomarbeit an der HTL Leonding** (Höhere Abteilung für Informatik), danach von mir überarbeitet und veröffentlicht: neue Datenbank (PostgreSQL/Neon), geschlossene Sicherheitslücken, automatisierte Tests, CI und Hosting auf Azure und Vercel.

🌐 **Live:** [ease-tech-ejts.vercel.app](https://ease-tech-ejts.vercel.app)

## Warum EaseTech?

Viele ältere Menschen stehen vor denselben Hürden: Geräte ohne verständliche Bedienungsanleitung, Tasten mit Mehrfachbelegung, fehlendes Wissen für faire Kaufentscheidungen und schwer auffindbare Ersatzteile. Bestehende Angebote sind oft unübersichtlich, bieten keinen Video-Support oder verlangen eine aufwendige Registrierung.

EaseTech setzt genau dort an: **wenig Hürden, persönliche Hilfe, verständliche Oberfläche.** Nutzen können die Plattform Privatpersonen ebenso wie Seniorenheime, Gemeinden oder Angehörige, die Hilfe für jemand anderen anfordern.

## Design für ältere Menschen

Die wichtigsten UX-Entscheidungen – bewusst einfach gehalten:

| Entscheidung | Warum |
|---|---|
| **Registrierung nur mit Vorname, Nachname, E-Mail und Passwort** | Jedes zusätzliche Feld ist eine Hürde. Adresse, Telefonnummer, Geburtsdatum usw. ergänzt die Person später im Profil – oder ein Mitarbeiter übernimmt das. |
| **Schlanke Navigation**: für Besucher nur *Anmelden*, *Registrieren* und *„Bitte hier drücken“* | Wer unsicher ist, landet mit einem Klick beim Kontaktformular und wird zurückgerufen. |
| **Schritt-für-Schritt statt Informationsflut**: zuerst nur **drei große Bilder** (Computer, Fernseher, Drucker) | Erst nach der Auswahl erscheinen die passenden Services, danach Preis und Dauer – und erst dann *„Jetzt buchen“*. Jeder Schritt zeigt nur, was gerade gebraucht wird. |
| **Persönliche Begrüßung** mit Vornamen | Die Person fühlt sich direkt angesprochen. |
| **Schwarz-Weiß-Farbschema, klare Schrift, große Schaltflächen** | Hoher Kontrast ist für ältere Augen leichter erkennbar. |
| **Kontaktformular auch für Dritte** | Angehörige oder Sozialarbeiter können Hilfe für eine andere Person anfordern. |
| **Passwort-Zurücksetzen per Telefon** statt per E-Mail-Link | Die Zielgruppe nutzt E-Mail oft wenig; ein Admin setzt ein neues Passwort, der Kunde bekommt es per Post. |
| **Termine vergibt das Team** statt Kalender-Auswahl für Kunden | Ein Buchungskalender wäre für unerfahrene Nutzer eher eine Hürde. |
| **Kunden-Dashboard mit nur drei Bereichen**: Profil, Meine Bestellungen, Passwort ändern | Übersichtlich statt überladen – Mitarbeiter und Admins sehen zusätzliche Menüpunkte je nach Rolle. |
| **Buchung nur innerhalb der Öffnungszeiten** | Außerhalb erscheint ein klarer Hinweis, der Buchen-Button wird ausgeblendet – niemand wartet vergeblich im Video-Call. |

## Architektur

```
┌──────────────────┐   HTTPS / JSON    ┌──────────────────┐   EF Core    ┌──────────────┐
│  Angular 19      │   (JWT-Bearer)    │  ASP.NET Core 8  │  (Npgsql)    │  PostgreSQL  │
│  Frontend        ├──────────────────▶│  Web API         ├─────────────▶│  (Neon,      │
│  (Vercel)        │                   │  (Azure App      │              │  serverless) │
└────────┬─────────┘                   │  Service)        │              └──────────────┘
         │                             └────────┬─────────┘
         │  WebRTC Video                        │ Token für Video-Call
         ▼                                      ▼
┌──────────────────────────────────────────────────────────┐
│                  GetStream Video (Cloud)                  │
└──────────────────────────────────────────────────────────┘
```

## Komponenten

| Ordner | Was | Tech-Stack |
|---|---|---|
| `Api/Api/` | REST-API mit Minimal APIs: Benutzer & Rollen, Services, Aufträge, Rechnungen, Kontaktanfragen, Video-Calls | ASP.NET Core 8, EF Core, ASP.NET Identity, JWT, Npgsql, Swagger |
| `Api/APITest/` | Integrationstests – startet die komplette API im Speicher mit einer In-Memory-Datenbank | MSTest, WebApplicationFactory |
| `Frontend/` | Web-Oberfläche: Startseite, Buchung, Kontakt, Video-Call und rollenbasiertes Dashboard | Angular 19 (Standalone Components), Bootstrap 5, ngx-toastr, GetStream Video SDK |
| `Frontend/e2e/` | End-to-End-Tests im Browser | Playwright |
| `.github/workflows/` | CI: Build, Tests und Migrations-Check bei jedem Push | GitHub Actions |
| `docs/` | Roadmap | Markdown |

## Features

- **Drei Rollen** – `Customer`, `Employee`, `Admin` – mit serverseitiger Rechteprüfung: Kunden sehen nur ihre eigenen Aufträge und Rechnungen, nur Admins legen Mitarbeiter an.
- **Services** für Computer, Fernseher und Drucker: Verbindungs- und Einstellungsprobleme, Reparatur- und Kaufberatung, sonstige Probleme – online oder vor Ort.
- **Buchung von Hilfe** direkt von der Startseite; der Preis wird **serverseitig** aus Minutenpreis × Dauer berechnet.
- **Video-Support**: Mitarbeiter schalten sich im Dashboard als verfügbar, Kunden werden mit einem freien Mitarbeiter verbunden (GetStream Video, Token wird von der API ausgestellt).
- **Aufträge & Rechnungen**: Mitarbeiter legen Aufträge an und bearbeiten den Status, abgeschlossene Aufträge erzeugen eine Rechnung mit Zahlungsreferenz und automatisch berechneter Umsatzsteuer (20 %).
- **Kundenverwaltung durch das Team**: Neue Kunden sind in der Übersicht farblich markiert, bis ein Mitarbeiter die fehlenden Daten ergänzt hat.
- **Kontaktformular** ohne Login – Anfragen landen im Dashboard und werden einem Mitarbeiter zugewiesen.
- **Sicherheit**: JWT mit begrenzter Laufzeit, Konto-Sperre nach 5 Fehlversuchen, Passwort-Richtlinie, Secrets nur über User-Secrets bzw. Umgebungsvariablen.
- **Automatische Datenbank-Einrichtung** beim Start: Migrationen, Rollen und optional ein erster Admin.
- **Tests & CI**: 63 API-Tests und 27 Angular-Unit-Tests laufen bei jedem Push über GitHub Actions.

## Lokal aufsetzen

**Voraussetzungen:** [.NET 8 SDK](https://dotnet.microsoft.com/download), [Node.js 22](https://nodejs.org) mit [pnpm](https://pnpm.io) und eine PostgreSQL-Datenbank (z. B. kostenlos bei [Neon](https://neon.tech) oder lokal per Docker).

### API

```bash
cd Api/Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=...;Database=...;Username=...;Password=...;SSL Mode=Require"
dotnet user-secrets set "AppSettings:JWT_Secret" "$(openssl rand -base64 48)"
dotnet run            # http://localhost:5001  ·  Swagger: /swagger
```

Beim ersten Start werden die Tabellen und Rollen automatisch angelegt.

<details>
<summary>Alle Einstellungen</summary>

Lokal über `dotnet user-secrets`, auf dem Server als Umgebungsvariablen (`:` wird dort zu `__`, z. B. `AppSettings__JWT_Secret`).

| Schlüssel | Pflicht | Bedeutung |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | ✅ | PostgreSQL Connection String (ADO.NET-Format) |
| `AppSettings:JWT_Secret` | ✅ | Schlüssel für die Tokens, mind. 32 Zeichen – sonst startet die API nicht |
| `AppSettings:TokenLifetimeHours` | | Gültigkeit eines Logins (Standard 12 h) |
| `Cors:AllowedOrigins` | | Erlaubte Frontend-URLs (Standard `http://localhost:4200`) |
| `Database:MigrateOnStartup` | | Migrationen beim Start anwenden (in *Development* an) |
| `Seed:AdminEmail` / `Seed:AdminPassword` | | Legt beim Start einen Admin an |
| `Seed:DemoData` / `Seed:DemoPassword` | | Demo-Services und Demo-Benutzer, nur lokal |
| `GetStream:ApiKey` / `GetStream:SecretKey` | für Video | Zugangsdaten von [getstream.io](https://getstream.io) |

Passwörter: mind. 8 Zeichen mit Groß- und Kleinbuchstaben, Ziffer und Sonderzeichen.

</details>

### Frontend

```bash
cd Frontend
pnpm install
pnpm start            # http://localhost:4200
```

Die API-Adresse steht in `src/environments/environment.development.ts` (lokal) bzw. `environment.ts` (Build für die Produktion).

### Tests

```bash
cd Api && dotnet test                 # API-Integrationstests
cd Frontend && pnpm run test:ci       # Angular-Unit-Tests (headless)
cd Frontend && pnpm run e2e           # Playwright (API + Frontend müssen laufen)
```

## Was ich überarbeitet habe

Das Diplomprojekt lief ursprünglich nur lokal mit SQL Server. Für die Veröffentlichung habe ich:

| Bereich | Vorher | Jetzt |
|---|---|---|
| Datenbank | SQL Server LocalDB (nur Windows) | PostgreSQL auf Neon, neue Migration, UTC-Zeitstempel |
| Secrets | JWT-Secret und API-Keys im Code | User-Secrets / Umgebungsvariablen, Start bricht ohne Secret ab |
| Registrierung | Jeder konnte sich selbst als **Admin** registrieren | Öffentlich nur `Customer`, Mitarbeiter/Admins nur durch Admins |
| Datenzugriff | Jeder eingeloggte Benutzer konnte fremde Profile, Aufträge und Rechnungen lesen/ändern | Nur eigene Daten; Listen nur für Mitarbeiter |
| Bestellungen | Preis und Status kamen vom Client | Server setzt Kunde, Status und berechnet den Preis |
| Login | Token 1 Monat gültig, unbegrenzte Versuche | 12 h, Sperre nach 5 Fehlversuchen |
| Tests | 4 Tests, nur mit lokaler Datenbank lauffähig | 63 API- + 27 Angular-Tests, laufen überall |
| Repository | `bin/`, `obj/`, Test-Ergebnisse versioniert | bereinigt, `.gitignore`, CI mit GitHub Actions |
| Frontend | Build schlug fehl, 31 Pakete (viele ungenutzt), 2,4 MB Bundle | Build repariert, 15 Pakete, 1,4 MB Bundle |
| Hosting | – | API auf Azure App Service, Frontend auf Vercel |

## Roadmap

Details und offene Entscheidungen in [`docs/ROADMAP.md`](docs/ROADMAP.md).

- [ ] **Einstellungen im Dashboard**: Öffnungszeiten pro Wochentag, Feiertage, Firmendaten, Steuersatz und Services statt fest im Code
- [ ] **Rechnung als PDF** zum Herunterladen und Ausdrucken
- [ ] **Feedback-Funktion** nach einem abgeschlossenen Service
- [ ] **Benachrichtigungen** – vor allem für das Team (neue Anfrage, neuer Auftrag); für Kunden nur optional, da die Zielgruppe E-Mail oft wenig nutzt
- [ ] **Video-Call verbessern**: Warteschlange, Video-Termine zu Aufträgen, Verfügbarkeit gekoppelt an die Öffnungszeiten
- [ ] **Barrierefreiheit prüfen** (WCAG): Schriftgröße umschaltbar, Tastaturbedienung, Screenreader
- [ ] **Mehr Tests**: Integrationstests gegen echtes PostgreSQL (Testcontainers), Playwright-E2E in der CI
- [ ] **Upgrade** auf .NET 10 (LTS) und aktuelles Angular vor dem Support-Ende von .NET 8 (11/2026)
- [ ] **Automatisches Deployment** der API per GitHub Actions
- [ ] Impressum & Datenschutzerklärung

## Über dieses Projekt

Ich bin Reza Jaghori. EaseTech war meine Diplomarbeit an der HTL Leonding (2025) – eine Plattform, die vor allem älteren Menschen schnell und unkompliziert bei technischen Problemen hilft, auf Wunsch per Video-Call.

Geplant war das Projekt ursprünglich zu zweit; ich war für das Backend und einzelne Oberflächen zuständig. Nachdem mein Teampartner die Schule verlassen hatte, habe ich das gesamte Projekt – Datenbank, API und Angular-Oberfläche – allein umgesetzt. Den ursprünglich in WPF geplanten Verwaltungsbereich habe ich dafür ebenfalls in Angular gebaut, damit alles eine einheitliche Oberfläche hat.

Nach dem Abschluss wollte ich das Projekt nicht in der Schublade liegen lassen, sondern so weiterentwickeln, wie man es in einem echten Team tun würde: Sicherheitslücken finden und schließen, automatisierte Tests schreiben, eine Cloud-Datenbank anbinden, CI einrichten und die Anwendung tatsächlich online bringen. Dabei ist mir klar geworden, wie groß der Schritt von „läuft auf meinem Rechner“ zu „läuft sicher und nachvollziehbar im Internet“ ist.

**Was ich dabei gelernt habe / gerade lerne:**
- **Sicherheit ist Aufgabe des Servers**: Ein ausgeblendeter Button im Frontend schützt nichts – jede Berechtigung muss die API prüfen. Ein Teil der Tests existiert genau dafür.
- **Secrets gehören nie ins Repository**: auslagern mit User-Secrets und Umgebungsvariablen, und was einmal öffentlich war, gilt als bekannt und muss ausgetauscht werden.
- **Testbarkeit**: Integrationstests mit `WebApplicationFactory` und In-Memory-Datenbank, damit die Tests ohne eigene Datenbank überall laufen – auch in der CI.
- **Deployment in der Praxis**: Azure App Service, Vercel und Neon einrichten, App Settings, CORS, SPA-Routing – und lernen, eine 404- oder 500-Fehlermeldung systematisch einzugrenzen.
- **Sauber mit Git arbeiten**: Branch-Strategie (`develop` → `main`), aussagekräftige Commits, `.gitignore`, Merge-Konflikte.

**Warum Neon, Azure und Vercel:** Alle drei haben einen brauchbaren Free-Tier, das Projekt kostet also nichts. Azure passt gut zum .NET-Ökosystem, Neon bietet serverloses PostgreSQL ohne eigene Server, und Vercel baut das Angular-Frontend bei jedem Push automatisch.

Ich bin auf Jobsuche als Entwickler im .NET-/C#- und Angular-Umfeld, im Raum Linz/Steyr.

Siehe auch mein zweites Projekt: [Dashboard-ESP32](https://github.com/JaGHori90/Dashboard-ESP32) – ein IoT-Dashboard vom Sensor bis zur Desktop-App.

## Lizenz

Dieses Projekt ist unter der [MIT-Lizenz](LICENSE) veröffentlicht.
