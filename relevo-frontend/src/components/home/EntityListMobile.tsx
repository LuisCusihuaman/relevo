import type { FC } from "react";
import { useTranslation } from "react-i18next";
import { GitBranch, MoreHorizontal } from "lucide-react";
import { handovers } from "../../pages/data";
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger } from "@/components/ui/dropdown-menu";

const monthMap: Record<string, string> = {
	Jan: "Ene",
	Feb: "Feb",
	Mar: "Mar",
	Apr: "Abr",
	May: "May",
	Jun: "Jun",
	Jul: "Jul",
	Aug: "Ago",
	Sep: "Sep",
	Oct: "Oct",
	Nov: "Nov",
	Dec: "Dic",
};

type EntityListMobileProps = {
	handleHandoverClick: (handoverId: string, projectName: string) => void;
};

export const EntityListMobile: FC<EntityListMobileProps> = ({
	handleHandoverClick,
}) => {
	const { t } = useTranslation("home");
	const mapEnvironment = (env: string): string => {
		if (env === "Unexpected Error") return "Evento crítico";
		if (env === "Promoted" || env === "Staged") return "Completado";
		return env;
	};

	const formatRelative = (value: string): string => {
		let s = value;
		s = s.replace(/(\d+)\s*d\s*ago/g, "hace $1 d");
		s = s.replace(/ago/g, "");
		s = s.replace(/\b(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)\b/g, (m) => monthMap[m] || m);
		return s.trim();
	};

	const formatAuthor = (name: string): string => {
		if (!name) return t("table.system");
		const lower = name.toLowerCase();
		if (lower.includes("[bot]") || lower.includes("dependabot")) return t("table.system");
		return name;
	};

	const getInitials = (fullName: string): string => {
		const cleaned = typeof fullName === "string" ? fullName.trim() : "";
		const parts = cleaned.split(/\s+/).filter(Boolean);
		const first = parts[0]?.[0] ?? "";
		const second = parts[1]?.[0] ?? "";
		const fallback = cleaned.slice(0, 2);
		const result = (first + second) || fallback || "PX";
		return result.toUpperCase();
	};

	const getTitleLine = (d: typeof handovers[number]): string => {
		if (d.bedLabel) return t("table.bed", { label: d.bedLabel });
		if (typeof d.mrn === "string" && d.mrn.length > 0) {
			const mrn: string = d.mrn;
			const short = mrn.length > 6 ? `${mrn.slice(-6, -2)}-${mrn.slice(-2)}` : mrn;
			return t("table.mrn", { value: short });
		}
		return t("table.noLocation");
	};

	return (
		<div className="md:hidden space-y-4">
			{handovers.map((handover) => (
				<div
					key={handover.id}
					className="bg-white border border-gray-200 rounded-lg p-4 space-y-3 cursor-pointer hover:border-gray-300 transition-colors"
					onClick={() => {
						handleHandoverClick(handover.id, handover.patientKey);
					}}
				>
					{/* Card Header */}
					<div className="flex items-center justify-between">
						<div>
							<h3 className="font-medium text-gray-900 text-base hover:underline cursor-pointer" title={t("table.locationTitle")}>
								{getTitleLine(handover)}
							</h3>
							<p className="text-sm text-gray-500" title={t("table.handoverType")}>
								{handover.environmentType}
							</p>
						</div>
						<DropdownMenu>
							<DropdownMenuTrigger asChild>
								<button className="h-6 w-6 p-0 text-gray-600 hover:text-gray-800 flex-shrink-0 flex items-center justify-center" title={t("table.more")} onClick={(event_) => { event_.stopPropagation(); }}>
									<MoreHorizontal className="h-4 w-4" />
								</button>
							</DropdownMenuTrigger>
							<DropdownMenuContent align="end">
								<DropdownMenuItem onClick={(event_) => { event_.stopPropagation(); void navigator.clipboard.writeText(handover.id); }}>{t("table.copyId")}</DropdownMenuItem>
							</DropdownMenuContent>
						</DropdownMenu>
					</div>

					{/* Status */}
					<div className="flex items-center gap-2">
						<span
							className={`h-2 w-2 rounded-full ${handover.statusColor}`}
							title={t("table.handoverType")}
						></span>
						<span className="text-sm font-medium text-gray-900">
							{handover.status}
						</span>
						<span className="text-sm text-gray-500">
							{formatRelative(handover.statusTime)}
						</span>
					</div>

					{/* Environment */}
					<div>
						<div
							className={`text-sm font-medium ${handover.environmentColor}`}
						>
							{mapEnvironment(handover.environment)}
						</div>
						<div className="text-sm text-gray-500">
							{formatRelative(handover.time)}
						</div>
					</div>

					{/* Patient Info */}
					<div className="flex items-center gap-2">
						<div
							className={`w-6 h-6 rounded-full flex items-center justify-center text-sm ${handover.patientIcon.bg} ${handover.patientIcon.text || "text-gray-700"}`}
						>
							{getInitials(handover.patientName)}
						</div>
						<span className="font-medium text-gray-900 text-sm hover:underline cursor-pointer">
							{handover.patientName}
						</span>
					</div>

					{/* Clinical Meta (single line) */}
					<div className="space-y-1">
						<div className="flex items-center gap-1 text-sm text-gray-600">
							<GitBranch className="h-4 w-4" />
							<span>{t("table.clinicalNotes")}</span>
						</div>
					</div>

					{/* Author */}
					<div className="flex items-center justify-between pt-2 border-t border-gray-100">
						<div className="flex items-center gap-2">
							<div className="w-6 h-6 rounded-full bg-blue-500 flex items-center justify-center text-xs font-medium text-white">
								{handover.avatar}
							</div>
							<span className="text-sm text-gray-600">
								{formatRelative(handover.time)} por {formatAuthor(handover.author)}
							</span>
						</div>
					</div>
				</div>
			))}
		</div>
	);
};
