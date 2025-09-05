import { Home } from "@/pages/Home";
import { createFileRoute, redirect } from "@tanstack/react-router";

export const Route = createFileRoute("/_authenticated/")({
	component: Home,
	beforeLoad() {
		const completed = window.localStorage.getItem("dailySetupCompleted") === "true";
		if (!completed) {
			redirect({ to: "/daily-setup", throw: true });
			return;
		}
	},
});
