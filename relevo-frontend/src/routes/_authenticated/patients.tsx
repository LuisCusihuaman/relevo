import { Patients } from "@/pages/Patients";
import { createFileRoute, redirect } from "@tanstack/react-router";

export const Route = createFileRoute("/_authenticated/patients")({
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
	},
	component: Patients,
});
