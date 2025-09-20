import { HandoverPage } from "@/pages/HandoverPage";
import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/_authenticated/$patientSlug/$handoverId")({
	component: HandoverPage,
});
