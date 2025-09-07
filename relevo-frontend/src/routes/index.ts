import { createFileRoute, redirect } from "@tanstack/react-router";

export const Route = createFileRoute("/")({
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

		// User is authenticated, redirect to dashboard
		throw redirect({
			to: "/dashboard",
			search: {},
		});
	},
});
