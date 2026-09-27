# Roadmap – vom Diplomprojekt zur veröffentlichten App

Gearbeitet wird auf `develop`. Erst wenn eine Phase fertig und getestet ist,
wird nach `main` gemergt.

## ✅ Phase 1 – Fundament (erledigt)

- Build-Artefakte (`bin/`, `obj/`, `.vs/`) aus dem Repository entfernt, `.gitignore` ergänzt
- **Secrets aus dem Code entfernt** (JWT-Secret, GetStream-Keys, Stream-API-Key im Frontend, Demo-Passwörter) → User-Secrets / Umgebungsvariablen
- **SQL Server → PostgreSQL (Neon)**, neue Initial-Migration, alle Zeitstempel als UTC
- Rollen und (optional) erster Admin werden beim Start automatisch angelegt, Demo-Daten für Development
- **Sicherheitslücken geschlossen** (Details unten)
- Konto-Sperre nach 5 Fehlversuchen, Passwortregeln (mind. 8 Zeichen) in Backend und Frontend gleich
- Neue Test-Suites: 63 API-Tests, 27 Angular-Tests; GitHub Actions CI
- Frontend-Build repariert (fehlendes `@popperjs/core`), Bug im Interceptor (`deleteToken` wurde nie aufgerufen)

### Geschlossene Sicherheitslücken

| Vorher | Jetzt |
|---|---|
| Jeder konnte sich über `/signup` selbst als **Admin** registrieren | Anonym nur `Customer`; nur Admins legen Mitarbeiter/Admins an |
| Jeder eingeloggte Benutzer konnte **jedes Profil ändern** (`/updateUserById`) | Nur eigenes Profil oder Mitarbeiter/Admin |
| Kontaktanfragen lesen/ändern ging **ohne Login** | Nur Mitarbeiter/Admin; Anlegen bleibt öffentlich |
| Kunden konnten alle Benutzer, Aufträge und Rechnungen abrufen | Nur eigene Daten; Listen nur für Mitarbeiter |
| Kunden konnten Aufträge mit beliebigem Preis/Status/Kunden anlegen | Server setzt Kunde, Status und berechnet den Preis |
| Jeder konnte einen Video-Call im Namen eines anderen registrieren | Nur Mitarbeiter, immer für sich selbst |
| Token 1 Monat gültig | 12 Stunden (konfigurierbar) |


---

## Phase 2 – E-Mail-Server (Benachrichtigungen & Passwort)

Ziel: Die API verschickt E-Mails über SMTP. Eine Schnittstelle `IEmailSender`
macht den Anbieter austauschbar (in Tests wird ein Fake verwendet).

- [ ] SMTP-Anbindung mit **MailKit**, Konfiguration über `Email:*` Secrets
- [ ] **Passwort vergessen**: Link mit Identity-Reset-Token per Mail → Seite „Neues Passwort setzen"
- [ ] E-Mail-Bestätigung bei der Registrierung
- [ ] Benachrichtigungen:
  - neue Kontaktanfrage → Mitarbeiter
  - Auftrag angelegt / Status geändert → Kunde
  - Rechnung erstellt → Kunde
  - Passwort wurde geändert → Benutzer (Sicherheitshinweis)
- [ ] Versand im Hintergrund (Queue), damit Requests nicht auf den Mailserver warten
- [ ] HTML-Vorlagen auf Deutsch

**Entscheidung offen – Mail-Anbieter:** z. B. Brevo (kostenloses Kontingent, SMTP),
Resend, Mailjet oder der SMTP-Server des eigenen Webhosters.

## Phase 3 – Einstellungen im Dashboard

Aktuell fest im Code: Öffnungszeiten (`home.component.ts`: 8–17 Uhr, Text sagt „bis 18 Uhr"),
Service-IDs der Buchungsbuttons (7, 2, 4, 8, 5, 9), Steuersatz 20 % (`InvoiceEndpoints`).

- [ ] Tabelle `BusinessSettings`: Firmenname, Adresse, UID, Steuersatz, Kontakt-E-Mail
- [ ] **Öffnungszeiten pro Wochentag** + Ausnahmen (Feiertage, Urlaub)
- [ ] Zeiten für Video-Support (kann von den Öffnungszeiten abweichen)
- [ ] Zuordnung Buchungskategorie → Service statt fester IDs
- [ ] Services im Dashboard anlegen/bearbeiten/löschen (API existiert bereits, UI fehlt)
- [ ] Admin-Seite „Einstellungen" im Dashboard, öffentlicher Endpunkt für Öffnungszeiten
- [ ] Startseite zeigt Öffnungszeiten live aus der API

## Phase 4 – Video-Call

Der Video-Call funktioniert bereits mit **GetStream Video**. Optionen:

| Anbieter | Vorteile | Nachteile |
|---|---|---|
| **GetStream** (aktuell) | schon integriert, im App-Design eingebettet | Konto/Keys nötig, Limits im Gratis-Tarif |
| **Jitsi Meet / JaaS** | Open Source, sehr einfach per iFrame | weniger Kontrolle über Design |
| **Google Meet** (Calendar API) | bekannte Oberfläche, Termin mit Meet-Link per Mail | benötigt Google Workspace; Call läuft außerhalb der App |
| **Daily.co** | einfache API, gutes Gratis-Kontingent | weiterer Anbieter |

Geplante Verbesserungen unabhängig vom Anbieter:
- [ ] Warteschlange: Kunde wartet, Mitarbeiter nimmt den nächsten Anruf an
- [ ] Video-Termine zu Aufträgen (Termin + Link per E-Mail aus Phase 2)
- [ ] Verfügbarkeit automatisch an die Öffnungszeiten aus Phase 3 koppeln

**Entscheidung offen:** bei GetStream bleiben oder wechseln?

## Phase 5 – Mehr Tests

- [ ] Integrationstests gegen echtes PostgreSQL (Testcontainers) statt nur In-Memory
- [ ] Playwright-E2E-Tests für die wichtigsten Abläufe (Registrierung, Buchung, Rechnung) in der CI
- [ ] Tests für jede neue Funktion aus Phase 2–4
- [ ] Code-Coverage-Bericht in der CI

## Phase 6 – Veröffentlichung

- [ ] Dockerfile für die API, Deployment (z. B. Render, Fly.io oder Azure App Service)
- [ ] Frontend-Hosting (z. B. Vercel, Netlify), Produktions-URL in `environment.ts`
- [ ] HTTPS, Rate-Limiting für Login/Registrierung, Health-Checks
- [ ] Lazy Loading der Angular-Routen (Initial-Bundle aktuell ~2,4 MB)
- [ ] Impressum und Datenschutzerklärung (DSGVO), Cookie-/Speicherhinweis
