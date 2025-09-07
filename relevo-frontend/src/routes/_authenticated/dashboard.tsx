import { Home } from "@/pages/Home";
import { createFileRoute, redirect } from "@tanstack/react-router";

export const Route = createFileRoute("/_authenticated/dashboard")({
	beforeLoad: ({ context }) => {
		// Check if user is authenticated
		if (!context.auth?.isLoaded) {
			// Clerk is still loading
			return;
		}

		if (!context.auth?.isSignedIn) {
			// User is not authenticated, redirect to login
			throw redirect({
				to: "/login",
				search: {},
			});
		}

		// Check if daily setup is completed
		const completed = window.localStorage.getItem("dailySetupCompleted") === "true";
		if (!completed) {
			throw redirect({
				to: "/daily-setup",
				search: {},
			});
		}
	},
	component: Home,
});
