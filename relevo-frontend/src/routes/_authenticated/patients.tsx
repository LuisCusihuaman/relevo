import { Patients } from "@/pages/Patients";
import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/_authenticated/patients")({
	component: Patients,
});
