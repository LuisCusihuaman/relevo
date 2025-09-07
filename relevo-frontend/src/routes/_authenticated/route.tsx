import { createFileRoute, redirect } from "@tanstack/react-router";
import { AppLayout } from "@/components/layout/AppLayout";
import { useAuth } from "@clerk/clerk-react";

function AuthenticatedRoute(): React.JSX.Element | null {
	const { isLoaded, isSignedIn } = useAuth();

	// Show loading state while Clerk is loading
	if (!isLoaded) {
		return (
			<div className="min-h-screen flex items-center justify-center">
				<div className="text-center">
					<div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
					<p className="mt-4 text-gray-600">Loading...</p>
				</div>
			</div>
		);
	}

	// If not signed in, redirect to login
	if (!isSignedIn) {
		redirect({ to: "/login", throw: true });
		return null;
	}

	// User is authenticated and loaded
	return <AppLayout />;
}

export const Route = createFileRoute("/_authenticated")({
	component: AuthenticatedRoute,
	beforeLoad: () => {
		// Additional authentication checks can be added here if needed
		return {};
	},
});
