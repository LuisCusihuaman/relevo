import { createRouter } from "@tanstack/react-router";
import React from "react";
import ReactDOM from "react-dom/client";
import App from "./App.tsx";
import { routeTree } from "./routeTree.gen.ts";
import "./index.css";
import "./common/i18n";
import { shadcn } from "@clerk/themes";
import { ClerkProvider, useAuth } from "@clerk/clerk-react";

// Import your Publishable Key
const PUBLISHABLE_KEY = import.meta.env["VITE_CLERK_PUBLISHABLE_KEY"] as string;

// Create a type for the router that will be created dynamically
export type TanstackRouter = ReturnType<typeof createRouter>;

// Create the router with initial context
let router: TanstackRouter;

// Create a router component that will have access to the auth context
export const RouterWithAuth: React.FC = () => {
	const auth = useAuth();

	// Create router only once or when auth changes
	if (
		!router ||
		(router.options.context as { auth?: typeof auth })?.auth !== auth
	) {
		router = createRouter({ routeTree, context: { auth } });
	}

	return <App router={router} />;
};

declare module "@tanstack/react-router" {
	interface Register {
		// This infers the type of our router and registers it across your entire project
		router: TanstackRouter;
	}
}

if (!PUBLISHABLE_KEY) {
	throw new Error("Add your Clerk Publishable Key to the .env file");
}

const rootElement = document.querySelector("#root") as Element;
if (!rootElement.innerHTML) {
	const root = ReactDOM.createRoot(rootElement);
	root.render(
		<React.StrictMode>
			<React.Suspense fallback="loading">
				<ClerkProvider
					afterSignOutUrl="/login"
					publishableKey={PUBLISHABLE_KEY}
					appearance={{
						baseTheme: shadcn,
						elements: {
							// Center only Clerk components (SignIn/SignUp) without affecting the whole app
							rootBox: "flex min-h-screen w-full items-center justify-center",
						},
					}}
				>
					<RouterWithAuth />
				</ClerkProvider>
			</React.Suspense>
		</React.StrictMode>
	);
}
