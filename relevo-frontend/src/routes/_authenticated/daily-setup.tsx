import { DailySetup } from "@/pages/DailySetup";
import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/_authenticated/daily-setup")({
	component: DailySetup,
});
