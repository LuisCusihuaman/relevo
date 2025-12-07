import { type ReactElement, useMemo } from "react";
import {
	DashboardSidebar,
	PatientDirectoryList,
	PatientDirectorySkeleton,
	PatientDirectoryToolbar,
	PatientProfileHeader,
	VersionNotice,
} from "@/components/home";
import { recentPreviews } from "@/pages/data";
import { useAssignedPatients, type PatientSummaryCard } from "@/api";
import { useUiStore } from "@/store/ui.store";

export type HomeProps = {
	patientSlug?: string;
};

export function Home(): ReactElement {
	const { currentPatient } = useUiStore();
	const { data: assignedPatientsData, isLoading, error } = useAssignedPatients();

	// Memoize the mapped patients to avoid unnecessary re-computations
	const patients: ReadonlyArray<PatientSummaryCard> = useMemo(() => {
		if (!assignedPatientsData?.items) return [];
		return assignedPatientsData.items;
	}, [assignedPatientsData]);

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
