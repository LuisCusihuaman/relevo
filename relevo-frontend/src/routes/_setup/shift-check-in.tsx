import { ShiftCheckIn } from "@/pages/ShiftCheckIn";
import { createFileRoute, redirect } from "@tanstack/react-router";

export const Route = createFileRoute("/_setup/shift-check-in")({
	beforeLoad: ({ context }) => {
		// Check if user is authenticated
		if (!context.auth?.isLoaded) {
			// Clerk is still loading
			return;
		}

		if (!context.auth?.isSignedIn) {
			// User is not authenticated, redirect to login
			redirect({
				to: "/login",
				search: {},
				throw: true,
			});
		}

		// Check if daily setup is already completed
		const completed = window.localStorage.getItem("dailySetupCompleted") === "true";
		if (completed) {
			// Setup already completed, redirect to dashboard
			redirect({
				to: "/dashboard",
				search: {},
				throw: true,
			});
		}
	},
	component: ShiftCheckIn,
});
