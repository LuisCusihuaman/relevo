import { createFileRoute, redirect } from "@tanstack/react-router";
import { SetupLayout } from "@/components/layout/AppLayout";

export const Route = createFileRoute("/_setup")({
	component: SetupLayout,
	async beforeLoad(context) {
		const token = await context.context.auth?.getToken();
		if (!token) {
			redirect({ to: "/login", throw: true });
		}
	},
});
