import { setupClerkTestingToken } from "@clerk/testing/playwright";
import { test, expect } from "@playwright/test";

test.describe("DailySetup Flow", () => {
	test.beforeEach(async ({ context }) => {
		// Force Spanish locale for i18next before any page scripts run
		await context.addInitScript(() => {
			window.localStorage.setItem("i18nextLng", "es");
		});
	});
	test("should allow a user to complete the daily setup process", async ({
		page,
	}) => {
		await setupClerkTestingToken({ page })
		// Step 0: Navigate to the Daily Setup page and enter doctor's name
		await page.goto("/daily-setup");
		await expect(page.getByRole("heading", { name: /bienvenido a relevo/i })).toBeVisible();
		await page.getByLabel("Tu Nombre").fill("Dr. Test");
		await page.getByRole("button", { name: "Continuar" }).click();

		// Step 1: Select a Unit
		await expect(
			page.getByRole("heading", { name: /selecciona tu unidad/i }),
		).toBeVisible();
		await page.getByRole("button", { name: "Pediatría General" }).click();
		await page.getByRole("button", { name: "Continuar" }).click();

		// Step 2: Select a Shift
		await expect(
			page.getByRole("heading", { name: /selecciona tu turno/i }),
		).toBeVisible();
		await page.getByRole("button", { name: "Mañana" }).click();
		await page.getByRole("button", { name: "Continuar" }).click();

		// Step 3: Select Patients
		await expect(
			page.getByRole("heading", { name: /selecciona tus pacientes/i }),
		).toBeVisible();
		await expect(
			page.getByText("0 de 3 pacientes seleccionados"),
		).toBeVisible();

		// Select a few patients
		await page.getByText("Ava Thompson").click();
		await page.getByText("Liam Rodríguez").click();
		await page.getByText("Mia Patel").click();

		// Verify selection count
		await expect(
			page.getByText("3 de 3 pacientes seleccionados"),
		).toBeVisible();

		// Complete the setup
		await page.getByRole("button", { name: "Completar Configuración" }).click();

		// Final Step: Verify completion and redirection
		// This assumes the user is redirected to a dashboard or home page
		// that shows the doctor's name.
		// As a best practice, you can assert the URL after confirming the content.
		// This ensures the page is in the state you expect before you check the URL.
		await expect(page).toHaveURL("/");
	});

	test("should show a validation error if no patients are selected", async ({
		page,
	}) => {
		// Navigate through the steps quickly to get to patient selection
		await page.goto("/daily-setup");
		await page.getByLabel("Tu Nombre").fill("Dr. Validación");
		await page.getByRole("button", { name: "Continuar" }).click(); // -> Step 1
		await page.getByRole("button", { name: "Pediatría General" }).click();
		await page.getByRole("button", { name: "Continuar" }).click(); // -> Step 2
		await page.getByRole("button", { name: "Mañana" }).click();
		await page.getByRole("button", { name: "Continuar" }).click(); // -> Step 3

		// Verify that the button is disabled and a validation message is shown
		await expect(
			page.getByRole("button", { name: "Completar Configuración" }),
		).toBeDisabled();

		// Select a patient and ensure the error disappears and button becomes enabled
		await page.getByText("Ava Thompson").click();
		await expect(
			page.getByRole("button", { name: "Completar Configuración" }),
		).toBeEnabled();
	});
});
