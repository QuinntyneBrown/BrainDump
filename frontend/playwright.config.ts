import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './e2e',
  fullyParallel: false,
  reporter: 'html',
  outputDir: 'test-results',
  use: {
    baseURL: 'http://localhost:4200',
    video: 'on',
    screenshot: 'only-on-failure',
    trace: 'on-first-retry',
  },
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],
  webServer: [
    {
      command: 'dotnet run --project ../backend/src/BrainDump.Api --launch-profile http',
      url: 'http://localhost:5153/openapi/v1.json',
      timeout: 60_000,
      reuseExistingServer: true,
    },
    {
      command: 'npx ng serve brain-dump',
      url: 'http://localhost:4200',
      timeout: 120_000,
      reuseExistingServer: true,
    },
  ],
});
