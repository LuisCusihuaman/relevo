import { defineConfig, devices } from "@playwright/test";
import { fileURLToPath } from "url";
import path from "path";
import dotenv from "dotenv";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const authFile = path.join(__dirname, "playwright/.clerk/user.json");

// Load environment variables globally for all tests from multiple possible locations
dotenv.config({ path: path.resolve(__dirname, "../.env") });
dotenv.config({ path: path.resolve(__dirname, ".env") });

export default defineConfig({
	testDir: "./e2e",
	/* Global setup to ensure environment variables are loaded */
	globalSetup: path.resolve(__dirname, "./e2e/global.setup.ts"),
	/* Run tests in files in parallel */
	fullyParallel: true,
	/* Fail the build on CI if you accidentally left test.only in the source code. */
	forbidOnly: !!process.env.CI,
	/* Retry on CI only */
	retries: process.env.CI ? 2 : 0,
	/* Opt out of parallel tests on CI. */
	workers: process.env.CI ? 1 : undefined,
	/* Reporter to use. See https://playwright.dev/docs/test-reporters */
	reporter: "html",
	/* Shared settings for all the projects below. See https://playwright.dev/docs/api/class-testoptions. */
	use: {
		/* Base URL to use in actions like `await page.goto('/')`. */
		baseURL: "http://localhost:5174",
		locale: "es",
		/* Collect trace when retrying the failed test. See https://playwright.dev/docs/trace-viewer */
		trace: "on-first-retry",
	},

	/* Configure projects for major browsers */
	projects: [
		{
			name: "chromium",
			use: { ...devices["Desktop Chrome"], storageState: authFile },
		},
	],

	/* Run your local dev server before starting the tests */
	webServer: {
	  command: 'pnpm run dev',
	  url: 'http://localhost:5174',
	  reuseExistingServer: !process.env.CI,
	},
});
