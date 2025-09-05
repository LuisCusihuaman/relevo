import { Home } from "@/pages/Home";
import { createFileRoute, redirect } from "@tanstack/react-router";

export const Route = createFileRoute("/_authenticated")({
    component: Home,
	async beforeLoad(context) {
		const token = await context.context.auth?.getToken();
		if (!token) {
			redirect({ to: "/login", throw: true });
			return;
		}
	},
});
