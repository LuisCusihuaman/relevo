import { type ReactElement, useEffect, useMemo } from "react";
import {
	DashboardSidebar,
	PatientDirectoryList,
	PatientDirectorySkeleton,
	PatientDirectoryToolbar,
	PatientProfileHeader,
	VersionNotice,
} from "@/components/home";
import { recentPreviews } from "@/pages/data";
import type { Patient } from "@/components/home/types";
import { useAssignedPatients, type PatientSummaryCard } from "@/api";
import { useUiStore } from "@/store/ui.store";

export type HomeProps = {
	patientSlug?: string;
};

// Mapping function to convert API PatientSummaryCard to UI Patient type
function mapPatientSummaryToPatient(patientCard: PatientSummaryCard): Patient {
	const getStatusFromHandoverStatus = (status: PatientSummaryCard["handoverStatus"]): string => {
		switch (status) {
			case "NotStarted":
				return "home:patientList.noHandover";
			case "InProgress":
				return "home:patientList.startHandover";
			case "Completed":
				return "home:patientList.handoverCompleted";
			default:
				return "home:patientList.noHandover";
		}
	};

	// Create a slug from the patient name for URL purposes
	const patientSlug = patientCard.name.toLowerCase().replace(/\s+/g, "-");

	return {
		id: patientCard.id,
		name: patientCard.name,
		url: patientSlug,
		status: getStatusFromHandoverStatus(patientCard.handoverStatus),
		date: new Date().toLocaleDateString("es-ES", { month: "short", day: "numeric" }),
		icon: patientCard.name.charAt(0).toUpperCase(),
		unit: "Assigned", // Default unit for assigned patients
		handoverId: patientCard.handoverId,
	};
}

export function Home({
	patientSlug,
}: HomeProps): ReactElement {
	const { currentPatient, actions } = useUiStore();
	const { data: assignedPatientsData, isLoading, error } = useAssignedPatients();

	// Memoize the mapped patients to avoid unnecessary re-computations
	const patients: ReadonlyArray<Patient> = useMemo(() => {
		if (!assignedPatientsData?.items) return [];
		return assignedPatientsData.items.map(mapPatientSummaryToPatient);
	}, [assignedPatientsData]);

	useEffect(() => {
		console.log("Home.tsx Debug:");
		console.log("- patientSlug:", patientSlug);
		console.log("- patients:", patients);
		const patient: Patient | null = patientSlug
			? patients.find((p: Patient): boolean => p.url === patientSlug) ?? null
			: null;
		console.log("- found patient:", patient);
		actions.setCurrentPatient(patient);
	}, [patientSlug, patients, actions]);

	const isPatientView: boolean = Boolean(currentPatient);

	// Show loading state
	if (isLoading) {
		return (
			<div className="flex-1 p-6">
				<div className="space-y-6">
					<VersionNotice />
					<div className="max-w-7xl mx-auto px-6 py-6">
						<PatientDirectoryToolbar />
						<div className="flex flex-col lg:flex-row gap-8">
							<DashboardSidebar recentPreviews={recentPreviews} />
							<PatientDirectorySkeleton />
						</div>
					</div>
				</div>
			</div>
		);
	}

	// Show error state
	if (error) {
		return (
			<div className="flex-1 p-6">
				<div className="space-y-6">
					<VersionNotice />
					<div className="max-w-7xl mx-auto px-6 py-6">
						<div className="flex items-center justify-center h-64">
							<div className="text-center">
								<div className="text-red-600 mb-2">Error al cargar pacientes</div>
								<p className="text-gray-600">Intente recargar la página</p>
							</div>
						</div>
					</div>
				</div>
			</div>
		);
	}

	return (
		<div className="flex-1 p-6">
			{!isPatientView && (
				<div className="space-y-6">
					<VersionNotice />
					<div className="max-w-7xl mx-auto px-6 py-6">
						<PatientDirectoryToolbar />
						<div className="flex flex-col lg:flex-row gap-8">
							<DashboardSidebar recentPreviews={recentPreviews} />

							<PatientDirectoryList patients={patients} />
						</div>
					</div>
				</div>
			)}

			{isPatientView && currentPatient ? (
				<div className="space-y-6">
					<PatientProfileHeader currentPatient={currentPatient} />
				</div>
			) : null}
		</div>
	);
}
