import type { FC } from "react";
import { useTranslation } from "react-i18next";
import { useNavigate } from "@tanstack/react-router";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Clock } from "lucide-react";
import { useUpcomingActions } from "@/hooks/useUpcomingActions";

type RecentActivityCardProps = {
	// Keep for backwards compatibility but not used
	recentPreviews?: Array<unknown>;
};

export const RecentActivityCard: FC<RecentActivityCardProps> = () => {
	const { t } = useTranslation("home");
	const navigate = useNavigate();
	const { upcomingActions, isLoading } = useUpcomingActions();

	const handleActionClick = (patientId: string, handoverId: string): void => {
		void navigate({ to: `/patient/${patientId}`, search: { handoverId } });
	};

	const formatDueTime = (dueTime: string | undefined): string => {
		if (!dueTime) return "";
		// If it's already a readable format, return as is
		if (dueTime.length < 20) return dueTime;
		// Otherwise try to format it
		return dueTime;
	};

	if (isLoading) {
		return (
			<div className="border border-gray-200 rounded-lg bg-white">
				<div className="p-6">
					<h3 className="text-base font-medium mb-4 leading-tight">{t("upcomingActions.title")}</h3>
					<div className="text-center py-8 text-gray-500">{t("upcomingActions.loading")}</div>
				</div>
			</div>
		);
	}

	return (
		<div className="border border-gray-200 rounded-lg bg-white">
			<div className="p-6">
				<h3 className="text-base font-medium mb-4 leading-tight">{t("upcomingActions.title")}</h3>
				<div className="space-y-0 divide-y divide-gray-100">
					{upcomingActions.length > 0 ? (
						upcomingActions.map((action) => (
							<div
								key={action.id}
								className="py-4 first:pt-0 last:pb-0"
							>
								<div className="flex items-start gap-3">
									<div className="flex-1 min-w-0">
										<p className="text-sm text-gray-900 mb-1 leading-tight font-medium">
											{action.patientName}
										</p>
										<p className="text-sm text-gray-600 mb-2 leading-tight font-normal">
											{action.description}
										</p>
										<div className="flex items-center gap-2 flex-wrap">
											{action.dueTime && (
												<Badge
													className="text-xs h-5 px-2 bg-blue-50 text-blue-700 hover:bg-blue-50 border-0 font-normal rounded flex items-center gap-1"
													variant="secondary"
												>
													<Clock className="h-3 w-3" />
													{formatDueTime(action.dueTime)}
												</Badge>
											)}
											{action.priority && (
												<Badge
													className={`text-xs h-5 px-2 border-0 font-normal rounded ${
														action.priority === "high"
															? "bg-red-50 text-red-700 hover:bg-red-50"
															: action.priority === "medium"
																? "bg-yellow-50 text-yellow-700 hover:bg-yellow-50"
																: "bg-gray-50 text-gray-700 hover:bg-gray-50"
													}`}
													variant="secondary"
												>
													{action.priority === "high"
														? t("upcomingActions.priority.high")
														: action.priority === "medium"
															? t("upcomingActions.priority.medium")
															: t("upcomingActions.priority.low")}
												</Badge>
											)}
											<Button
												className="h-6 px-2 text-xs text-gray-600 hover:text-gray-900 font-normal bg-gray-50 hover:bg-gray-100 rounded border border-gray-200"
												size="sm"
												variant="ghost"
												onClick={() => handleActionClick(action.patientId, action.handoverId)}
											>
												<svg
													className="h-3 w-3 mr-1"
													fill="currentColor"
													viewBox="0 0 20 20"
												>
													<path d="M10 12a2 2 0 100-4 2 2 0 000 4z" />
													<path
														clipRule="evenodd"
														d="M.458 10C1.732 5.943 5.522 3 10 3s8.268 2.943 9.542 7c-1.274 4.057-5.064 7-9.542 7S1.732 14.057.458 10zM14 10a4 4 0 11-8 0 4 4 0 018 0z"
														fillRule="evenodd"
													/>
												</svg>
												{t("recentActivity.open")}
											</Button>
										</div>
									</div>
								</div>
							</div>
						))
					) : (
						<div className="text-center py-8 text-gray-500">{t("upcomingActions.empty")}</div>
					)}
				</div>
			</div>
		</div>
	);
};
