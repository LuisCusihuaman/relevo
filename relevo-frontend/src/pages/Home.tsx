import { type ReactElement, useMemo, useState } from "react";
import {
	DashboardSidebar,
	PatientDirectoryList,
	PatientDirectorySkeleton,
	PatientDirectoryToolbar,
	PatientProfileHeader,
	VersionNotice,
} from "@/components/home";
import { recentPreviews } from "@/pages/data";
import { useAssignedPatients } from "@/api";
import type { PatientSummaryCard } from "@/types/domain";
import { useUiStore } from "@/store/ui.store";

export type HomeProps = {
	patientSlug?: string;
};

export function Home(): ReactElement {
	const { currentPatient } = useUiStore();
	const { data: assignedPatientsData, isLoading, error } = useAssignedPatients();
	const [searchTerm, setSearchTerm] = useState("");

	// Memoize the mapped patients to avoid unnecessary re-computations
	const allPatients: ReadonlyArray<PatientSummaryCard> = useMemo(() => {
		if (!assignedPatientsData?.items) return [];
		return assignedPatientsData.items;
	}, [assignedPatientsData]);

	// Filter patients based on search term
	const patients: ReadonlyArray<PatientSummaryCard> = useMemo(() => {
		if (!searchTerm.trim()) return allPatients;
		const lowerSearchTerm = searchTerm.toLowerCase().trim();
		return allPatients.filter((patient) =>
			patient.name.toLowerCase().includes(lowerSearchTerm)
		);
	}, [allPatients, searchTerm]);

	const isPatientView: boolean = Boolean(currentPatient);

	// Show loading state
	if (isLoading) {
		return (
			<div className="flex-1 p-6">
				<div className="space-y-6">
					<VersionNotice patients={[]} />
					<div className="max-w-7xl mx-auto px-6 py-6">
						<PatientDirectoryToolbar />
						<div className="flex flex-col lg:flex-row gap-8">
							<DashboardSidebar recentPreviews={recentPreviews} patients={[]} />
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
					<VersionNotice patients={[]} />
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
					<VersionNotice patients={allPatients} />
					<div className="max-w-7xl mx-auto px-6 py-6">
						<PatientDirectoryToolbar searchTerm={searchTerm} onSearchChange={setSearchTerm} />
						<div className="flex flex-col lg:flex-row gap-8">
							<DashboardSidebar recentPreviews={recentPreviews} patients={allPatients} />

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
