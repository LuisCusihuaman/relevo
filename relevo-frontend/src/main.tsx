import { createRouter } from "@tanstack/react-router";
import React from "react";
import ReactDOM from "react-dom/client";
import App from "./App.tsx";
import { routeTree } from "./routeTree.gen.ts";
import "./index.css";
import "./common/i18n";
import { ClerkProvider } from "@clerk/clerk-react";

const router = createRouter({ routeTree, context: { auth: undefined } });

export type TanstackRouter = typeof router;

declare module "@tanstack/react-router" {
	interface Register {
		// This infers the type of our router and registers it across your entire project
		router: TanstackRouter;
	}
}
// Import your Publishable Key
const PUBLISHABLE_KEY = import.meta.env["VITE_CLERK_PUBLISHABLE_KEY"] as string;

if (!PUBLISHABLE_KEY) {
	throw new Error("Add your Clerk Publishable Key to the .env file");
}

const rootElement = document.querySelector("#root") as Element;
if (!rootElement.innerHTML) {
	const root = ReactDOM.createRoot(rootElement);
	root.render(
		<React.StrictMode>
			<React.Suspense fallback="loading">
				<ClerkProvider afterSignOutUrl="/" publishableKey={PUBLISHABLE_KEY}>
					<App router={router} />
				</ClerkProvider>
			</React.Suspense>
		</React.StrictMode>
	);
}
