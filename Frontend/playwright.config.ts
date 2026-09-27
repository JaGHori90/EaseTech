// playwright.config.ts
import { defineConfig } from '@playwright/test';

export default defineConfig({
  testDir: './e2e', // ⬅ wichtig: verweist auf den richtigen Ordner
  use: {
    baseURL: 'http://localhost:4200', // Deine Angular-App
  },
});
