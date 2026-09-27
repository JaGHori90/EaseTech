import { test, expect } from '@playwright/test'

test('Kontaktformular sollte erfolgreich abgeschickt werden', async ({ page }) => {

    await page.goto('/page/contact');

    //Formular ausfüllen
    await page.fill('input[formcontrolname=referrer]', 'Max Mustermann');
    await page.fill('input[formcontrolname="name"]', 'John Doe');
    await page.fill('input[formcontrolname="phoneNumber"]', '+43 732 123456');
    await page.fill('textarea[formcontrolname="message"]', 'Ich habe ein Anliegen.');

    await page.click('button[type="submit"]');

});