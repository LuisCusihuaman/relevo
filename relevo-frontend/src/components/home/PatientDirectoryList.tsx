import {type FC, useState, useEffect } from "react";
import { useNavigate } from "@tanstack/react-router";
import {
	DropdownMenu,
	DropdownMenuContent,
	DropdownMenuItem,
	DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Activity, GitBranch, MoreHorizontal, Loader2 } from "lucide-react";
import { useTranslation } from "react-i18next";
import { usePatientHandoverTimeline } from "@/api/endpoints/patients";
import type { Patient } from "./types";

export type PatientDirectoryListProps = {
	patients: ReadonlyArray<Patient>;
};

export const PatientDirectoryList: FC<PatientDirectoryListProps> = ({ patients }: PatientDirectoryListProps) => {
	const { t } = useTranslation("home");
	const navigate = useNavigate();
	const [selectedPatientId, setSelectedPatientId] = useState<string | null>(null);

	// Get handover timeline for the selected patient
	const { data: handoverTimeline, isLoading: loadingHandovers } = usePatientHandoverTimeline(
		selectedPatientId || "",
		{ pageSize: 50 } // Get recent handovers
	);

	// Handle navigation when handover data is loaded
	useEffect(() => {
		if (handoverTimeline?.items && handoverTimeline.items.length > 0 && selectedPatientId) {
			// Find the active handover (Active or InProgress status)
			const activeHandover = handoverTimeline.items.find(
				(item) => item.status === "Active" || item.status === "InProgress"
			);

			if (activeHandover) {
				// Find the patient to get the URL slug
				const patient = patients.find((p: Patient) => p.id === selectedPatientId);
				if (patient && activeHandover.id) {
					console.log("Navigating to active handover:", activeHandover.id);
					void navigate({
						to: "/$patientSlug/$handoverId",
						params: {
							patientSlug: patient.url,
							handoverId: activeHandover.id,
						},
					});
				}
			} else {
				// No active handover found
				console.log("No active handover found for patient:", selectedPatientId);
				alert("Este paciente no tiene un handover activo en este momento.");
			}

			// Reset selected patient
			setSelectedPatientId(null);
		}
	}, [handoverTimeline, selectedPatientId, patients, navigate]);

	const formatDate = (dateString: string): string => {
		if (!/^[a-zA-Z]{3}\s\d{1,2}$/.test(dateString)) {
			return dateString;
		}
		const [month, day] = dateString.split(" ");
		return `${day} ${month}`;
	};

	const getTranslatedStatus = (status: string): string => {
		if (status.startsWith("home:")) {
			const key = status.replace("home:", "");
			return String(t(key));
		}
		return status;
	};

	const handlePatientClick = (patient: Patient): void => {
		console.log("Patient clicked:", patient);
		// Set the selected patient to trigger handover lookup
		setSelectedPatientId(patient.id);
	};

	// Show loading indicator for the selected patient
	const isLoadingPatient = loadingHandovers && selectedPatientId;

	return (
		<div className="flex-1 min-w-0">
			<div className="mb-4">
				<h2 className="text-base font-medium leading-tight">{t("patientList.title")}</h2>
			</div>
			<div className="bg-white border border-gray-200 rounded-xl shadow-sm overflow-hidden">
				<ul className="divide-y divide-gray-200">
					{patients.length > 0 ? (
						patients.map((patient: Patient) => (
							<li
								key={patient.id}
								className={`grid grid-cols-[minmax(0,2fr)_minmax(0,2fr)_minmax(0,1fr)] items-center gap-6 py-4 px-6 hover:bg-gray-50 cursor-pointer transition-colors ${
									isLoadingPatient && selectedPatientId === patient.id ? 'bg-blue-50' : ''
								}`}
								onClick={() => {
									handlePatientClick(patient);
								}}
							>
								<div className="flex items-center gap-3 min-w-0">
									<span className="h-10 w-10 rounded-full bg-gray-100 flex items-center justify-center shrink-0">
										{patient.icon}
									</span>
									<div className="min-w-0 flex-1">
										<div className="text-sm font-medium text-gray-900 truncate flex items-center gap-2">
											{patient.name}
											{isLoadingPatient && selectedPatientId === patient.id && (
												<Loader2 className="h-3 w-3 animate-spin text-blue-600" />
											)}
										</div>
										<div className="text-xs text-gray-600 truncate">
											{getTranslatedStatus(patient.status)}
										</div>
									</div>
								</div>

								<div className="min-w-0 flex-1">
									<a className="block text-sm font-medium text-blue-600 hover:text-blue-700 hover:underline truncate">
										{t("patientList.startHandover")}
									</a>
									<div className="mt-1 text-xs text-gray-600 flex items-center gap-2">
										<time>{formatDate(patient.date)}</time>
										{patient.unit && (
											<>
												<span>{t("patientList.in")}</span>
												<span className="inline-flex items-center gap-1">
													<GitBranch className="h-3.5 w-3.5" />
													{patient.unit}
												</span>
											</>
										)}
									</div>
								</div>

								<div className="flex items-center justify-end gap-2 shrink-0 min-w-0">
									{/* Placeholder for future GitHub/status indicator */}
									<div className="w-[120px]"></div>
									<button
										className="h-8 w-8 rounded-full text-gray-400 hover:bg-gray-50 shrink-0 flex items-center justify-center"
										title={String(t("patientList.actionList"))}
									>
										<Activity className="h-4 w-4" />
									</button>
									<DropdownMenu>
										<DropdownMenuTrigger asChild>
											<button
												className="h-8 w-8 rounded-full text-gray-600 hover:bg-gray-50 shrink-0 flex items-center justify-center"
												title={String(t("patientList.more"))}
											>
												<MoreHorizontal className="h-4 w-4" />
											</button>
										</DropdownMenuTrigger>
										<DropdownMenuContent align="end">
											<DropdownMenuItem>{t("patientList.open")}</DropdownMenuItem>
											<DropdownMenuItem>{t("patientList.viewNotes")}</DropdownMenuItem>
											<DropdownMenuItem>
												{t("patientList.startHandover")}
											</DropdownMenuItem>
										</DropdownMenuContent>
									</DropdownMenu>
								</div>
							</li>
						))
					) : (
						<li className="text-center py-8 text-gray-500">
							{t("patientList.noPatientsFound")}{" "}
							<a className="text-blue-600 hover:underline" href="#">
								{t("patientList.changeFilters")}
							</a>
						</li>
					)}
				</ul>
			</div>
		</div>
	);
};
