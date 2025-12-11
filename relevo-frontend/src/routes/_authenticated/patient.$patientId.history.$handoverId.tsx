import HistoricalHandoverPage from "@/pages/historical-handover";
import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/_authenticated/patient/$patientId/history/$handoverId")({
	component: HistoricalHandoverPage,
});
