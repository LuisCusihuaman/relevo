import { createFileRoute, Outlet, redirect } from "@tanstack/react-router";
import type { ReactElement } from "react";

function RouteComponent(): ReactElement {
	return (
		<div>
			Hello from "/_authenticated"! Layout
			<Outlet />
		</div>
	);
}

export const Route = createFileRoute("/_authenticated")({
	component: RouteComponent,
	async beforeLoad(context) {
		const token = await context.context.auth?.getToken();
		if (!token) {
			redirect({ to: "/login", throw: true });
			return;
		}
	},
});
