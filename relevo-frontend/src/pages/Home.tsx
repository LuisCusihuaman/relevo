import { type ReactElement, useEffect, useState } from "react";
import {
	DashboardSidebar,
	PatientDirectoryList,
	PatientDirectoryToolbar,
	PatientProfileHeader,
	VersionNotice,
} from "@/components/home";
import { patients, recentPreviews } from "@/pages/data";
import type { Patient } from "@/components/home/types";
import { useUiStore } from "@/store/ui.store";

export type HomeProps = {
	patientSlug?: string;
	initialTab?: string;
};

export function Home({
	patientSlug,
	initialTab = "summary",
}: HomeProps): ReactElement {
	const [activeTab, setActiveTab] = useState<string>(initialTab);
	const { currentPatient, actions } = useUiStore();

	useEffect(() => {
		// Normalize legacy Spanish labels to new keys
		if (initialTab === "Resumen") setActiveTab("summary");
		else if (initialTab === "Ajustes") setActiveTab("settings");
		else setActiveTab(initialTab);
	}, [initialTab]);

	useEffect(() => {
		const patientsList: ReadonlyArray<Patient> =
			patients as ReadonlyArray<Patient>;
		const patient: Patient | null = patientSlug
			? patientsList.find((p: Patient): boolean => p.name === patientSlug) ??
			  null
			: null;
		actions.setCurrentPatient(patient);
	}, [patientSlug, actions]);

	const isPatientView: boolean = Boolean(currentPatient);

	return (
		<div className="flex-1 p-6">
			{!isPatientView && activeTab === "summary" && (
				<div className="space-y-6">
					<VersionNotice />
					<div className="max-w-7xl mx-auto px-6 py-6">
						<PatientDirectoryToolbar />
						<div className="flex flex-col lg:flex-row gap-8">
							<DashboardSidebar recentPreviews={recentPreviews} />

							<PatientDirectoryList />
						</div>
					</div>
				</div>
			)}

			{isPatientView && currentPatient ? (
				<div className="space-y-6">
					{activeTab === "summary" && (
						<PatientProfileHeader currentPatient={currentPatient} />
					)}
				</div>
			) : null}
		</div>
	);
}
