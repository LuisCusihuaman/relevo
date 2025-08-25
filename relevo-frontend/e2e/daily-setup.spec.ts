import { test, expect } from "@playwright/test";

test.describe("DailySetup Flow", () => {
	test("should allow a user to complete the daily setup process", async ({
		page,
	}) => {
		// Step 0: Navigate to the Daily Setup page and enter doctor's name
		await page.goto("/daily-setup");
		await expect(
			page.getByRole("heading", { name: "Welcome to Relevo" }),
		).toBeVisible();
		await page.getByPlaceholder("e.g., Dr. Jane Doe").fill("Dr. Test");
		await page.getByRole("button", { name: "Continue" }).click();

		// Step 1: Select a Unit
		await expect(
			page.getByRole("heading", { name: "Hello, Dr. Test!" }),
		).toBeVisible();
		await expect(
			page.getByRole("heading", { name: "Select Your Unit" }),
		).toBeVisible();
		await page.getByRole("button", { name: "General Pediatrics" }).click();
		await page.getByRole("button", { name: "Continue" }).click();

		// Step 2: Select a Shift
		await expect(
			page.getByRole("heading", { name: "Select Your Shift" }),
		).toBeVisible();
		await page.getByRole("button", { name: "Morning Shift" }).click();
		await page.getByRole("button", { name: "Continue" }).click();

		// Step 3: Select Patients
		await expect(
			page.getByRole("heading", { name: "Select Your Patients" }),
		).toBeVisible();
		await expect(page.getByText("0 of 12 selected")).toBeVisible();

		// Select a few patients
		await page.getByText("Liam Johnson").click();
		await page.getByText("Olivia Williams").click();
		await page.getByText("Noah Brown").click();

		// Verify selection count
		await expect(page.getByText("3 of 12 selected")).toBeVisible();

		// Complete the setup
		await page.getByRole("button", { name: "Complete Setup" }).click();

		// Final Step: Verify completion and redirection
		// This assumes the user is redirected to a dashboard or home page
		// that shows the doctor's name.
		await expect(
			page.getByRole("heading", { name: "Welcome back, Dr. Test!" }),
		).toBeVisible();

		// As a best practice, you can assert the URL after confirming the content.
		// This ensures the page is in the state you expect before you check the URL.
		await expect(page).toHaveURL("/");
	});

	test("should show a validation error if no patients are selected", async ({
		page,
	}) => {
		// Navigate through the steps quickly to get to patient selection
		await page.goto("/daily-setup");
		await page.getByPlaceholder("e.g., Dr. Jane Doe").fill("Dr. Validation");
		await page.getByRole("button", { name: "Continue" }).click(); // -> Step 1
		await page.getByRole("button", { name: "General Pediatrics" }).click();
		await page.getByRole("button", { name: "Continue" }).click(); // -> Step 2
		await page.getByRole("button", { name: "Morning Shift" }).click();
		await page.getByRole("button", { name: "Continue" }).click(); // -> Step 3

		// Verify that the button is disabled and a validation message is shown
		await expect(
			page.getByRole("button", { name: "Complete Setup" }),
		).toBeDisabled();
		await expect(
			page.getByText("Please select at least one patient"),
		).toBeVisible();

		// Select a patient and ensure the error disappears and button becomes enabled
		await page.getByText("Liam Johnson").click();
		await expect(
			page.getByText("Please select at least one patient"),
		).not.toBeVisible();
		await expect(
			page.getByRole("button", { name: "Complete Setup" }),
		).toBeEnabled();
	});
});
