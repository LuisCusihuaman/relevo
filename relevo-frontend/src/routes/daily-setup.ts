import { createFileRoute } from "@tanstack/react-router";
import { DailySetup } from "../pages/DailySetup";

export const Route = createFileRoute("/daily-setup")({
	component: DailySetup,
});
