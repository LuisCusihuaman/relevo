import PatientHandoverPage from "@/pages/patient-handover";
import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/_authenticated/patient/$patientId")({
	component: PatientHandoverPage,
});
