import type { FC } from "react";
import { GitBranch, MoreHorizontal } from "lucide-react";
import { deployments, projectToPatientName } from "../../pages/data";
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger } from "@/components/ui/dropdown-menu";

type EntityListMobileProps = {
	handleDeploymentClick: (deploymentId: string, projectName: string) => void;
};

export const EntityListMobile: FC<EntityListMobileProps> = ({
	handleDeploymentClick,
}) => {
	const mapStatusText = (status: string): string => {
		if (status === "Error") return "Crítico";
		if (status === "Ready") return "Completado";
		if (status === "Promoted") return "Completado";
		if (status === "Staged") return "Completado";
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

	const mapEnvironment = (env: string): string => {
		if (env === "Unexpected Error") return "Evento crítico";
		if (env === "Promoted" || env === "Staged") return "Completado";
		return env;
	};

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

	const mapPatientName = (techName: string): string =>
		projectToPatientName[techName] || techName;

	const getInitials = (fullName: string): string => {
		const cleaned = typeof fullName === "string" ? fullName.trim() : "";
		const parts = cleaned.split(/\s+/).filter(Boolean);
		const first = parts[0]?.[0] ?? "";
		const second = parts[1]?.[0] ?? "";
		const fallback = cleaned.slice(0, 2);
		const result = (first + second) || fallback || "PX";
		return result.toUpperCase();
	};

	const getTitleLine = (d: typeof deployments[number]): string => {
		if (d.bedLabel) return `Cama ${d.bedLabel}`;
		if (typeof d.mrn === "string" && d.mrn.length > 0) {
			const mrn: string = d.mrn;
			const short = mrn.length > 6 ? `${mrn.slice(-6, -2)}-${mrn.slice(-2)}` : mrn;
			return `MRN · ${short}`;
		}
		return "Sin ubicación";
	};

	return (
		<div className="md:hidden space-y-4">
			{deployments.map((deployment) => (
				<div
					key={deployment.id}
					className="bg-white border border-gray-200 rounded-lg p-4 space-y-3 cursor-pointer hover:border-gray-300 transition-colors"
					onClick={() => {
						handleDeploymentClick(deployment.id, deployment.project);
					}}
				>
					{/* Card Header */}
					<div className="flex items-center justify-between">
						<div>
							<h3 className="font-medium text-gray-900 text-base hover:underline cursor-pointer" title="Ubicación del paciente">
								{getTitleLine(deployment)}
							</h3>
							<p className="text-sm text-gray-500" title="Tipo de sesión de traspaso">
								{mapEnvType(deployment.environmentType)}
							</p>
						</div>
						<DropdownMenu>
							<DropdownMenuTrigger asChild>
								<button className="h-6 w-6 p-0 text-gray-600 hover:text-gray-800 flex-shrink-0 flex items-center justify-center" title="Más" onClick={(e) => e.stopPropagation()}>
									<MoreHorizontal className="h-4 w-4" />
								</button>
							</DropdownMenuTrigger>
							<DropdownMenuContent align="end">
								<DropdownMenuItem onClick={(e) => { e.stopPropagation(); void navigator.clipboard.writeText(deployment.id); }}>Copiar ID</DropdownMenuItem>
							</DropdownMenuContent>
						</DropdownMenu>
					</div>

					{/* Status */}
					<div className="flex items-center gap-2">
						<span
							className={`h-2 w-2 rounded-full ${deployment.statusColor}`}
							title="Estado del traspaso"
						></span>
						<span className="text-sm font-medium text-gray-900">
							{mapStatusText(deployment.status)}
						</span>
						<span className="text-sm text-gray-500">
							{formatRelative(deployment.statusTime)}
						</span>
					</div>

					{/* Environment */}
					<div>
						<div
							className={`text-sm font-medium ${deployment.environmentColor}`}
						>
							{mapEnvironment(deployment.environment)}
						</div>
						<div className="text-sm text-gray-500">
							{formatRelative(deployment.time)}
						</div>
					</div>

					{/* Project Info */}
					<div className="flex items-center gap-2">
						<div
							className={`w-6 h-6 rounded-full flex items-center justify-center text-sm ${deployment.projectIcon.bg} ${deployment.projectIcon.text || "text-gray-700"}`}
						>
							{getInitials(mapPatientName(deployment.project))}
						</div>
						<span className="font-medium text-gray-900 text-sm hover:underline cursor-pointer">
							{mapPatientName(deployment.project)}
						</span>
					</div>

					{/* Clinical Meta (single line) */}
					<div className="space-y-1">
						<div className="flex items-center gap-1 text-sm text-gray-600">
							<GitBranch className="h-4 w-4" />
							<span>Notas clínicas</span>
						</div>
					</div>

					{/* Author */}
					<div className="flex items-center justify-between pt-2 border-t border-gray-100">
						<div className="flex items-center gap-2">
							<div className="w-6 h-6 rounded-full bg-blue-500 flex items-center justify-center text-xs font-medium text-white">
								{deployment.avatar}
							</div>
							<span className="text-sm text-gray-600">
								{formatRelative(deployment.time)} por {formatAuthor(deployment.author)}
							</span>
						</div>
					</div>
				</div>
			))}
		</div>
	);
};
