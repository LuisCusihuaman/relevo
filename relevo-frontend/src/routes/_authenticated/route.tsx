import { createFileRoute, redirect } from "@tanstack/react-router";

export const Route = createFileRoute("/_authenticated")({
	async beforeLoad(context) {
		const token = await context.context.auth?.getToken();
		if (!token) {
			redirect({ to: "/login" });
			return;
		}
	},
});
