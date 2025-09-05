import { clerk, clerkSetup } from "@clerk/testing/playwright";
import { chromium, FullConfig } from "@playwright/test";
import path from "path";
import { fileURLToPath } from "url";

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const authFile = path.join(__dirname, "../playwright/.clerk/user.json");

async function globalSetup(config: FullConfig): Promise<void> {
	const { VITE_CLERK_PUBLISHABLE_KEY, E2E_CLERK_USER_EMAIL, E2E_CLERK_USER_PASSWORD } = process.env;
	
	if (!VITE_CLERK_PUBLISHABLE_KEY || !E2E_CLERK_USER_EMAIL || !E2E_CLERK_USER_PASSWORD) {
		throw new Error("Missing required environment variables. Check .env file for VITE_CLERK_PUBLISHABLE_KEY, E2E_CLERK_USER_EMAIL, E2E_CLERK_USER_PASSWORD");
	}

	await clerkSetup({ publishableKey: VITE_CLERK_PUBLISHABLE_KEY });

	const browser = await chromium.launch();
	const page = await browser.newPage();

	const baseURL = config.projects?.[0]?.use?.baseURL || "http://localhost:5174/";
	await page.goto(baseURL);

	await clerk.signIn({
		page,
		signInParams: {
			strategy: "password",
			identifier: E2E_CLERK_USER_EMAIL,
			password: E2E_CLERK_USER_PASSWORD,
		},
	});

	await page.context().storageState({ path: authFile });
	await browser.close();
}

export default globalSetup;
