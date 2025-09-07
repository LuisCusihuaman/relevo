import type { FC } from "react";
import { GitBranch, MoreHorizontal } from "lucide-react";
import { handovers } from "../../pages/data";
import type { Handover } from "./types";
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger } from "@/components/ui/dropdown-menu";

type EntityTableProps = {
	handleHandoverClick: (handoverId: string, projectName: string) => void;
};

export const EntityTable: FC<EntityTableProps> = ({
	handleHandoverClick,
}) => {
	const mapStatusText = (status: string): string => {
		if (status === "Error") return "Crítico";
		if (status === "Ready") return "Completado";
		if (status === "Promoted") return "Completado"; // verde → Completado
		if (status === "Staged") return "Completado"; // verde → Completado
		switch (status) {
			case "Queued":
			case "Pending":
				return "No iniciado";
			case "Running":
			case "In progress":
				return "En progreso";
			case "Succeeded":
			case "Completed":
				return "Completado";
			case "Failed":
				return "Fallido";
			default:
				return status;
		}
	};

	const mapEnvType = (type: string): string => {
		if (type === "Preview") return "Tipo: Vista";
		if (type === "Production") return "Tipo: Actual";
		return `Tipo: ${type}`;
	};

	// environment label now shown only via status/time; keep mapping if needed later

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

	const formatRelative = (value: string): string => {
		let s = value;
		s = s.replace(/(\d+)\s*d\s*ago/g, "hace $1 d");
		s = s.replace(/ago/g, "");
		s = s.replace(/\b(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)\b/g, (m) => monthMap[m] || m);
		return s.trim();
	};

	const formatAuthor = (name: string): string => {
		if (!name) return "sistema";
		const lower = name.toLowerCase();
		if (lower.includes("[bot]") || lower.includes("dependabot")) return "sistema";
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

	const isString = (v: unknown): v is string => typeof v === "string";

	const getTitleLine = (d: Handover): string => {
		const bed = isString(d.bedLabel) ? String(d.bedLabel) : undefined;
		if (bed && bed !== "") return `Cama ${bed}`;

		const mrnValue = isString(d.mrn) ? String(d.mrn) : undefined;
		if (mrnValue && mrnValue !== "") {
			const short: string = mrnValue.length > 6 ? `${mrnValue.slice(-6, -2)}-${mrnValue.slice(-2)}` : mrnValue;
			return `MRN · ${short}`;
		}
		return "Sin ubicación";
	};

	const handoversList: ReadonlyArray<Handover> = handovers as ReadonlyArray<Handover>;

	return (
		<div className="hidden md:block rounded-lg border border-gray-200 bg-white overflow-hidden">
			{handoversList.map((handover, index) => (
				<div
					key={handover.id}
					className={`grid grid-cols-[1fr_1fr_1fr_1fr_1fr] items-center gap-4 py-3 px-4 hover:bg-gray-50 transition-colors cursor-pointer ${
						index < 5 ? "border-b border-gray-100" : ""
					}`}
					onClick={() => {
						handleHandoverClick(handover.id, handover.patientKey);
					}}
				>
					{/* Left Column: Location/MRN instead of technical ID */}
					<div className="min-w-0">
						<div className="font-medium text-gray-900 text-sm hover:underline cursor-pointer truncate" title="Ubicación del paciente">
							{getTitleLine(handover)}
						</div>
						<div className="text-xs text-gray-500 mt-0.5" title="Tipo de sesión de traspaso">
							{mapEnvType(handover.environmentType)}
						</div>
					</div>

					{/* Status Column */}
					<div className="min-w-0">
						<div className="flex items-center gap-2 mb-1">
							<span
								className={`h-2 w-2 rounded-full ${handover.statusColor}`}
								title="Estado del traspaso"
							></span>
							<span className="text-sm font-medium text-gray-900">
								{mapStatusText(handover.status)}
							</span>
						</div>
						<div className="text-xs text-gray-500">
							{formatRelative(handover.statusTime)}
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
									<button className="h-6 w-6 p-0 text-gray-600 hover:text-gray-800 flex-shrink-0 flex items-center justify-center" title="Más" onClick={(event_) => { event_.stopPropagation(); }}>
										<MoreHorizontal className="h-4 w-4" />
									</button>
								</DropdownMenuTrigger>
								<DropdownMenuContent align="end">
									<DropdownMenuItem onClick={(event_) => { event_.stopPropagation(); void navigator.clipboard.writeText(handover.id); }}>Copiar ID</DropdownMenuItem>
								</DropdownMenuContent>
							</DropdownMenu>
						</div>
						<div className="flex items-center gap-1 text-xs text-gray-500">
							<GitBranch className="h-3 w-3" />
							<span className="truncate">Notas clínicas</span>
						</div>
					</div>

					{/* Created Column */}
					<div className="min-w-0 text-right">
						<div className="flex items-center justify-end gap-2">
							<GitBranch className="h-3 w-3 text-gray-400" />
							<span className="text-xs text-gray-500">
								{formatRelative(handover.time)} por {formatAuthor(handover.author)}
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
