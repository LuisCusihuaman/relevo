import { DailySetup } from "@/pages/DailySetup";
import { createFileRoute, redirect } from "@tanstack/react-router";

export const Route = createFileRoute("/_setup/daily-setup")({
	component: DailySetup,
	beforeLoad() {
		const completed = window.localStorage.getItem("dailySetupCompleted") === "true";
		if (completed) {
			redirect({ to: "/", throw: true });
		}
	},
});