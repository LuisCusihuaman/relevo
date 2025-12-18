import type { FC } from "react";
import { useTranslation } from "react-i18next";
import { GitBranch, MoreHorizontal } from "lucide-react";
import type { HandoverUI as Handover } from "@/types/domain";
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger } from "@/components/ui/dropdown-menu";
import { getInitials } from "@/lib/formatters";

type EntityTableProps = {
	handovers: ReadonlyArray<Handover>;
	handleHandoverClick: (handoverId: string, projectName: string) => void;
	loading?: boolean;
	unitName?: string;
};

export const EntityTable: FC<EntityTableProps> = ({
	handovers,
	handleHandoverClick,
	loading = false,
	unitName,
}) => {
	const { t } = useTranslation("home");
	// environment label now shown only via status/time; keep mapping if needed later

	const formatAuthor = (name: string | undefined): string => {
		if (!name || name === "System") return String(t("table.unassigned"));
		const lower = name.toLowerCase();
		if (lower.includes("[bot]") || lower.includes("dependabot")) return String(t("table.unassigned"));
		return name;
	};

	const isString = (v: unknown): v is string => typeof v === "string";

	const getTitleLine = (d: Handover): string => {
		// Show unit name from patient data (priority)
		const unitValue = isString(d.unit) ? String(d.unit) : undefined;
		if (unitValue && unitValue !== "") {
			return unitValue;
		}

		// Fallback to unitName prop if available (when filtering by unit)
		if (unitName && unitName !== "") {
			return unitName;
		}

		const mrnValue = isString(d.mrn) ? String(d.mrn) : undefined;
		if (mrnValue && mrnValue !== "") {
			const short: string = mrnValue.length > 6 ? `${mrnValue.slice(-6, -2)}-${mrnValue.slice(-2)}` : mrnValue;
			return String(t("table.mrn", { value: short }));
		}
		return String(t("table.noLocation"));
	};

	const handoversList: ReadonlyArray<Handover> = handovers;

	// Show skeleton during loading
	if (loading) {
		const skeletonRows = Array.from({ length: 5 }, (_, index) => index);
		return (
			<div className="hidden md:block rounded-lg border border-gray-200 bg-white overflow-hidden">
				{skeletonRows.map((index) => (
					<div
						key={`desktop-${index}`}
						className={`grid grid-cols-[1fr_1fr_1fr_1fr] items-center gap-4 py-3 px-4 ${
							index < 4 ? "border-b border-gray-100" : ""
						}`}
					>
						{/* Left Column: Location/MRN */}
						<div className="min-w-0 space-y-2">
							<div className="h-4 bg-gray-200 rounded animate-pulse w-3/4"></div>
							<div className="h-3 bg-gray-100 rounded animate-pulse w-1/2"></div>
						</div>

						{/* Patient/Source Column */}
						<div className="min-w-0 space-y-2">
							<div className="flex items-center gap-2">
								<div className="h-5 w-5 bg-gray-200 rounded animate-pulse"></div>
								<div className="h-4 bg-gray-200 rounded animate-pulse w-20"></div>
								<div className="h-6 w-6 bg-gray-200 rounded animate-pulse"></div>
							</div>
							<div className="flex items-center gap-1">
								<div className="h-3 w-3 bg-gray-200 rounded animate-pulse"></div>
								<div className="h-3 bg-gray-200 rounded animate-pulse w-24"></div>
							</div>
						</div>

						{/* Created Column */}
						<div className="min-w-0 text-right space-y-2">
							<div className="flex items-center justify-end gap-2">
								<div className="h-3 w-3 bg-gray-200 rounded animate-pulse"></div>
								<div className="h-3 bg-gray-200 rounded animate-pulse w-16"></div>
								<div className="h-6 w-6 bg-gray-200 rounded-full animate-pulse"></div>
							</div>
						</div>
					</div>
				))}
			</div>
		);
	}

	return (
		<div className="hidden md:block rounded-lg border border-gray-200 bg-white overflow-hidden">
			{handoversList.map((handover, index) => (
				<div
					key={handover.id}
					className={`grid grid-cols-[1fr_1fr_1fr_1fr] items-center gap-4 py-3 px-4 hover:bg-gray-50 transition-colors cursor-pointer ${
						index < 5 ? "border-b border-gray-100" : ""
					}`}
					onClick={() => {
						handleHandoverClick(handover.id, handover.patientKey);
					}}
				>
					{/* Left Column: Location/MRN instead of technical ID */}
					<div className="min-w-0">
						<div className="font-medium text-gray-900 text-sm hover:underline cursor-pointer truncate" title={String(t("table.locationTitle"))}>
							{getTitleLine(handover)}
						</div>
						<div className="text-xs text-gray-500 mt-0.5" title={String(t("table.handoverType"))}>
							{t("filterToolbar.medicalUnit")}
						</div>
					</div>

					{/* Patient/Source Column */}
					<div className="min-w-0">
						<div className="flex items-center gap-2 mb-1">
							<div
								className={`w-5 h-5 rounded-full flex items-center justify-center text-xs ${handover.patientIcon.bg} ${handover.patientIcon.text || "text-gray-700"}`}
							>
								{getInitials(handover.patientName)}
							</div>
							<span className="font-medium text-gray-900 text-sm hover:underline cursor-pointer truncate">
								{handover.patientName}
							</span>
							<DropdownMenu>
								<DropdownMenuTrigger asChild>
									<button className="h-6 w-6 p-0 text-gray-600 hover:text-gray-800 flex-shrink-0 flex items-center justify-center" title={String(t("table.more"))} onClick={(event_) => { event_.stopPropagation(); }}>
										<MoreHorizontal className="h-4 w-4" />
									</button>
								</DropdownMenuTrigger>
								<DropdownMenuContent align="end">
									<DropdownMenuItem onClick={(event_) => { event_.stopPropagation(); void navigator.clipboard.writeText(handover.id); }}>{String(t("table.copyId"))}</DropdownMenuItem>
								</DropdownMenuContent>
							</DropdownMenu>
						</div>
						<div className="flex items-center gap-1 text-xs text-gray-500">
							<GitBranch className="h-3 w-3" />
							<span className="truncate">{String(t("table.clinicalNotes"))}</span>
						</div>
					</div>

					{/* Assigned User Column */}
					<div className="min-w-0 text-right">
						<div className="flex items-center justify-end gap-2">
							<span className="text-xs text-gray-500">
								{formatAuthor(handover.author)}
							</span>
							<div className="w-6 h-6 rounded-full bg-blue-500 flex items-center justify-center text-xs font-medium text-white">
								{handover.avatar}
							</div>
						</div>
					</div>
				</div>
			))}
		</div>
	);
};
