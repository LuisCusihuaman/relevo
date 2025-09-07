import { createFileRoute, redirect } from "@tanstack/react-router";
import { AppLayout } from "@/components/layout/AppLayout";

export const Route = createFileRoute("/_authenticated")({
	component: AppLayout,
	async beforeLoad(context) {
		const token = await context.context.auth?.getToken();
		if (!token) {
			redirect({ to: "/login", throw: true });
		}
	},
});
