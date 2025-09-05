import { test, expect } from "@playwright/test";

test.describe("DailySetup Flow", () => {
	test("should allow a user to complete the daily setup process", async ({
		page,
	}) => {
		await page.goto("/daily-setup");
		await page.getByRole('button', { name: 'Continuar' }).click();
		await page.getByRole('button', { name: 'Pediatría General' }).click();
		await page.getByRole('button', { name: 'Continuar' }).click();
		await page.getByRole('button', { name: 'Noche 19:00 - 07:' }).click();
		await page.getByRole('button', { name: 'Continuar' }).click();
		// Validate counter starts at 0 of 3 on patients step
		await expect(page.getByText('0 de 3 pacientes seleccionados')).toBeVisible();
		await page.getByRole('button', { name: 'Liam Rodríguez Edad N/D •' }).click();
		await page.getByRole('button', { name: 'Ava Thompson Edad N/D •' }).click();
		// Validate counter updates to 2 of 3 and button becomes enabled
		await expect(page.getByText('2 de 3 pacientes seleccionados')).toBeVisible();
		await expect(page.getByRole('button', { name: 'Completar Configuración' })).toBeEnabled();
		await page.getByRole('button', { name: 'Completar Configuración' }).click();
		await expect(page.getByRole('heading', { name: 'Welcome back, Doctora' })).toBeVisible();
		await page.close();
	});

	test("should keep completion disabled until at least one patient is selected", async ({
		page,
	}) => {
		await page.goto("/daily-setup");
		await page.getByRole('button', { name: 'Continuar' }).click();
		await page.getByRole('button', { name: 'Pediatría General' }).click();
		await page.getByRole('button', { name: 'Continuar' }).click();
		await page.getByRole('button', { name: 'Noche 19:00 - 07:' }).click();
		await page.getByRole('button', { name: 'Continuar' }).click();

		// Validate initial counter and disabled state
		await expect(page.getByText('0 de 3 pacientes seleccionados')).toBeVisible();
		await expect(page.getByRole('button', { name: 'Completar Configuración' })).toBeDisabled();

		// Select one patient -> counter 1 of 3 and enabled
		await page.getByRole('button', { name: 'Ava Thompson Edad N/D •' }).click();
		await expect(page.getByText('1 de 3 pacientes seleccionados')).toBeVisible();
		await expect(page.getByRole('button', { name: 'Completar Configuración' })).toBeEnabled();

		// Deselect the same patient -> counter back to 0 of 3 and disabled
		await page.getByRole('button', { name: 'Ava Thompson Edad N/D •' }).click();
		await expect(page.getByText('0 de 3 pacientes seleccionados')).toBeVisible();
		await expect(page.getByRole('button', { name: 'Completar Configuración' })).toBeDisabled();
        await page.close();
	});
});


