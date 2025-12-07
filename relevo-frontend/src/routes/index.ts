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
		// eslint-disable-next-line @typescript-eslint/only-throw-error
	throw redirect({
				to: "/login",
				search: {},
			});
		}

		// User is authenticated, redirect to dashboard
	// eslint-disable-next-line @typescript-eslint/only-throw-error
	throw redirect({
			to: "/dashboard",
			search: {},
		});
	},
});
