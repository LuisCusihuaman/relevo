import type { FC } from "react";
import { useTranslation } from "react-i18next";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { MoreHorizontal } from "lucide-react";
import type { RecentPreview } from "./types";

type RecentActivityCardProps = {
	recentPreviews: Array<RecentPreview>;
};

export const RecentActivityCard: FC<RecentActivityCardProps> = ({
	recentPreviews,
}) => {
    const { t } = useTranslation("home");
	const handleActionClick = (): void => {
		// Agrega aquí la lógica para el deep-link
		alert("Abriendo detalles...");
	};

	return (
		<div className="border border-gray-200 rounded-lg bg-white">
			<div className="p-6">
				<h3 className="text-base font-medium mb-4 leading-tight">{t("recentActivity.title")}</h3>
				<div className="space-y-0 divide-y divide-gray-100">
					{recentPreviews.length > 0 ? (
						recentPreviews.map((preview, index) => (
							<div
								key={index}
								className="py-4 first:pt-0 last:pb-0"
							>
								<div className="flex items-center gap-3">
									<div className="flex -space-x-1 flex-shrink-0">
										{preview.avatars.map((avatar, index_) => (
											<Avatar
												key={index_}
												className="h-6 w-6 border-2 border-white"
											>
												<AvatarImage
													src={avatar.src || "/placeholder.svg"}
												/>
												<AvatarFallback
													className={`${avatar.bg} text-white text-xs font-medium`}
												>
													{avatar.fallback}
												</AvatarFallback>
											</Avatar>
										))}
									</div>
									<div className="flex-1 min-w-0">
										<p className="text-sm text-gray-900 mb-2 leading-tight font-normal">
											{t(preview.title)}
										</p>
										<div className="flex items-center gap-2 flex-wrap">
											<Button
												className="h-6 px-2 text-xs text-gray-600 hover:text-gray-900 font-normal bg-gray-50 hover:bg-gray-100 rounded border border-gray-200"
												size="sm"
												variant="ghost"
												onClick={handleActionClick}
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
											{preview.status === "Source" && (
												<Badge
													className="text-xs h-5 px-2 bg-gray-100 text-gray-700 hover:bg-gray-200 border-0 font-normal rounded"
													variant="secondary"
												>
													{t("recentActivity.source")}
												</Badge>
											)}
											{preview.pr && (
												<span className="text-xs text-gray-500 font-normal">
													{preview.pr}
												</span>
											)}
											{preview.color === "Ready" && (
												<Badge
													className="text-xs h-5 px-2 bg-green-50 text-green-700 hover:bg-green-50 border-0 font-normal rounded"
													variant="secondary"
												>
													{t("recentActivity.ready")}
												</Badge>
											)}
											{preview.status === "Error" && (
												<Badge
													className="text-xs h-5 px-2 bg-red-50 text-red-600 hover:bg-red-50 border-0 font-normal rounded"
													variant="destructive"
												>
													● {t("recentActivity.error")}
												</Badge>
											)}
										</div>
									</div>
									<Button
										className="h-6 w-6 p-0 text-gray-600 hover:text-gray-800 flex-shrink-0 flex items-center justify-center"
										size="sm"
										variant="ghost"
									>
										<MoreHorizontal className="h-4 w-4" />
									</Button>
								</div>
							</div>
						))
					) : (
						<div className="text-center py-8 text-gray-500">{t("recentActivity.empty")}</div>
					)}
				</div>
			</div>
		</div>
	);
};
