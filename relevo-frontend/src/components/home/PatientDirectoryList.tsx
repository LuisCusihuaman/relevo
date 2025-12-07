import { type FC, useState } from "react";
import { useNavigate } from "@tanstack/react-router";
import { useQueryClient } from "@tanstack/react-query";
import {
	DropdownMenu,
	DropdownMenuContent,
	DropdownMenuItem,
	DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Activity, GitBranch, MoreHorizontal, Loader2 } from "lucide-react";
import { useTranslation } from "react-i18next";
import { patientHandoverTimelineQueryOptions } from "@/api/endpoints/patients";
import type { Patient } from "./types";

export type PatientDirectoryListProps = {
	patients: ReadonlyArray<Patient>;
};

export const PatientDirectoryList: FC<PatientDirectoryListProps> = ({ patients }: PatientDirectoryListProps) => {
	const { t } = useTranslation("home");
	const navigate = useNavigate();
	const queryClient = useQueryClient();
	const [loadingPatientId, setLoadingPatientId] = useState<string | null>(null);

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

	// Direct navigation pattern - fetch and navigate in one action
	const handlePatientClick = async (patient: Patient): Promise<void> => {
		if (loadingPatientId) return; // Prevent double-clicks

		setLoadingPatientId(patient.id);

		try {
			// Fetch handover timeline directly (not via useEffect)
			const handoverTimeline = await queryClient.fetchQuery(
				patientHandoverTimelineQueryOptions(patient.id, { pageSize: 50 })
			);

			if (handoverTimeline?.items && handoverTimeline.items.length > 0) {
				const mostRecentHandover = handoverTimeline.items[0];

				if (mostRecentHandover?.id) {
					await navigate({
						to: "/$patientSlug/$handoverId",
						params: {
							patientSlug: patient.url,
							handoverId: mostRecentHandover.id,
						},
					});
					return;
				}
			}

			// No handover found - show user-friendly message
			alert(t("patientList.noHandoversFound"));
		} catch (error) {
			console.error("Error fetching handovers:", error);
			alert(t("patientList.errorLoadingHandovers"));
		} finally {
			setLoadingPatientId(null);
		}
	};

	const isLoadingPatient = (patientId: string): boolean => loadingPatientId === patientId;

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
									isLoadingPatient(patient.id) ? 'bg-blue-50' : ''
								}`}
								onClick={() => { void handlePatientClick(patient); }}
							>
								<div className="flex items-center gap-3 min-w-0">
									<span className="h-10 w-10 rounded-full bg-gray-100 flex items-center justify-center shrink-0">
										{patient.icon}
									</span>
									<div className="min-w-0 flex-1">
										<div className="text-sm font-medium text-gray-900 truncate flex items-center gap-2">
											{patient.name}
											{isLoadingPatient(patient.id) && (
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
									{/* Placeholder for future status indicator */}
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
