# Test info

- Name: Kontaktformular sollte erfolgreich abgeschickt werden
- Location: C:\Users\Sasan JAGHORI\source\repos\JaGHori90\Maturaproject\EaseTech\EaseTech\e2e\contact\contact-form.spec.ts:3:5

# Error details

```
Error: page.fill: Test timeout of 30000ms exceeded.
Call log:
  - waiting for locator('input[formcontrolname=referrer]')

    at C:\Users\Sasan JAGHORI\source\repos\JaGHori90\Maturaproject\EaseTech\EaseTech\e2e\contact\contact-form.spec.ts:8:16
```

# Page snapshot

```yaml
- navigation:
  - link "EaseTech Logo EaseTech":
    - /url: /page/home
    - img "EaseTech Logo"
    - text: EaseTech
  - button "Anmelden"
  - button "Registrieren"
- heading "EaseTech" [level=4]
- heading "Damberggasse 2, 4030 Linz" [level=6]
- heading "📞 0732 123456" [level=6]:
  - text: 📞
  - link "0732 123456":
    - /url: tel:+43732123456
- heading "📧 easetech-service@gmail.com" [level=6]:
  - text: 📧
  - link "easetech-service@gmail.com":
    - /url: mailto:easetech-service@gmail.com
- heading "Sollen wir Sie, Ihre Eltern oder Ihre Patienten anrufen? Bitte füllen Sie dazu dieses Formular aus." [level=6]
- text: "Empfohlen von:"
- textbox "Empfohlen von ..."
- text: "Name:"
- textbox "Ihr Name"
- text: "Telefonnummer:"
- textbox "+43 732 123456"
- text: "Bitte schildern Sie kurz Ihr Problem:"
- textbox "Ihr Problem"
- button "Absenden"
- contentinfo:
  - paragraph: © 2024 EaseTech Service Company. All rights reserved.
  - link "AGB":
    - /url: "#"
  - link "Datenschutz":
    - /url: "#"
```

# Test source

```ts
   1 | import { test, expect } from '@playwright/test'
   2 |
   3 | test('Kontaktformular sollte erfolgreich abgeschickt werden', async ({ page }) => {
   4 |
   5 |     await page.goto('/page/contact');
   6 |
   7 |     //Formular ausfüllen
>  8 |     await page.fill('input[formcontrolname=referrer]', 'Max Mustermann');
     |                ^ Error: page.fill: Test timeout of 30000ms exceeded.
   9 |     await page.fill('input[formcontrolname="name"]', 'John Doe');
  10 |     await page.fill('input[formcontrolname="phoneNumber"]', '+43 732 123456');
  11 |     await page.fill('textarea[formcontrolname="message"]', 'Ich habe ein Anliegen.');
  12 |
  13 |     await page.click('button[type="submit"]');
  14 |
  15 | });
```