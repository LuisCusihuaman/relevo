import { clerk, clerkSetup } from "@clerk/testing/playwright";
import { test as setup } from "@playwright/test";
import path from "path";
import { fileURLToPath } from "url";
const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

// Ensures that Clerk setup is done before any tests run
setup.describe.configure({
	mode: "serial",
});

setup("global setup", async () => {
	await clerkSetup();
	if (
		!process.env["E2E_CLERK_USER_EMAIL"] ||
		!process.env["E2E_CLERK_USER_PASSWORD"]
	) {
		throw new Error(
			"Please provide E2E_CLERK_USER_EMAIL and E2E_CLERK_USER_PASSWORD environment variables."
		);
	}
}); // Close previous setup

const authFile = path.join(__dirname, "../playwright/.clerk/user.json");

setup("authenticate", async ({ page }) => {
	await page.goto("/");
	await clerk.signIn({
		page,
		signInParams: {
			strategy: "password",
			identifier: process.env["E2E_CLERK_USER_EMAIL"]!,
			password: process.env["E2E_CLERK_USER_PASSWORD"]!,
		},
	});

	await page.context().storageState({ path: authFile });
});
