import type { FC } from "react";
import { useTranslation } from "react-i18next";
import { Button } from "@/components/ui/button";
import { ChevronDown } from "lucide-react";
import {
	Popover,
	PopoverContent,
	PopoverTrigger,
} from "@/components/ui/popover";
import type { Metric } from "./types";
import { metrics } from "@/pages/data";


export const SectionMeta: FC = () => {
	const { t } = useTranslation("home");
	return (
		<div>
			<h2 className="text-base font-medium mb-4 leading-tight">
				{t("sectionMeta.title")}
			</h2>
			<div className="border border-gray-200 rounded-lg bg-white">
				<div className="p-6">
					<div className="flex items-center justify-between mb-6">
						<div>
							<p className="text-base font-medium text-gray-900 leading-tight">
								{t("sectionMeta.last30Days")}
							</p>
							<p className="text-sm text-gray-600 mt-1 leading-tight">
								{t("sectionMeta.updatedAgo")}
							</p>
						</div>
						<Button
							className="bg-black text-white hover:bg-gray-800 h-8 px-3 text-sm font-medium"
							size="sm"
						>
							{t("sectionMeta.viewMore")}
						</Button>
					</div>

					<div className="space-y-3">
						{metrics.map((metric: Metric) => (
							<Popover key={metric.label}>
								<PopoverTrigger asChild>
									<div className="flex items-center justify-between py-2 cursor-pointer">
										<div className="flex items-center gap-3">
											<div className="relative w-3 h-3">
												<svg
													className="w-3 h-3 transform -rotate-90"
													viewBox="0 0 36 36"
												>
													<path
														d="M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831"
														fill="none"
														stroke="#f3f4f6"
														strokeWidth="4"
													/>
													<path
														d="M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831"
														fill="none"
														stroke="#3b82f6"
														strokeDasharray={`${(parseInt(metric.currentValue) / parseInt(metric.totalValue)) * 100}, 100`}
														strokeWidth="4"
													/>
												</svg>
											</div>
											<span className="text-sm text-gray-900 leading-tight">
												{t(metric.label)}
											</span>
										</div>
										<span className="text-sm text-gray-600 font-mono leading-tight">
											{metric.value}
										</span>
									</div>
								</PopoverTrigger>
								<PopoverContent>
									<p className="text-sm">{t(metric.tooltip)}</p>
								</PopoverContent>
							</Popover>
						))}

						<div className="flex justify-center pt-3">
							<Button
								className="h-6 w-6 p-0 text-gray-400 hover:text-gray-600"
								size="sm"
								variant="ghost"
							>
								<ChevronDown className="h-4 w-4" />
							</Button>
						</div>
					</div>
				</div>
			</div>
		</div>
	);
};
