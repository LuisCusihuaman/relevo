import type { FC } from "react";
import {
	DropdownMenu,
	DropdownMenuContent,
	DropdownMenuItem,
	DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Activity, GitBranch, Github, MoreHorizontal } from "lucide-react";
import { patients as patients } from "../../pages/data";

export const PatientDirectoryList: FC = () => {
	const getStatusText = (status: string): string => {
		switch (status) {
			case "No Production Deployment":
				return "Sin traspaso activo";
			case "Create deploy.yml":
				return "Crear plan de traspaso";
			case "docs: README.md":
				return "Notas clínicas";
			case "Initial commit":
				return "Ingreso inicial";
			default:
				return status;
		}
	};

	const getActionText = (status: string): string => {
		if (status.includes("traspaso")) {
			return "Continuar traspaso";
		}
		if (status.includes("Notas")) {
			return "Notas clínicas";
		}
		return "Iniciar traspaso";
	};

	const formatDate = (dateString: string): string => {
		if (!/^[a-zA-Z]{3}\s\d{1,2}$/.test(dateString)) {
			return dateString;
		}
		const [month, day] = dateString.split(" ");
		return `Ingresó: ${day} ${month}`;
	};

	return (
		<div className="flex-1 min-w-0">
			<div className="mb-4">
				<h2 className="text-base font-medium leading-tight">Mis Pacientes</h2>
			</div>
			<div className="bg-white border border-gray-200 rounded-xl shadow-sm overflow-hidden">
				<ul className="divide-y divide-gray-200">
					{patients.length > 0 ? (
						patients.map((patient, index) => (
							<li
								key={index}
								className="grid grid-cols-[minmax(0,2fr)_minmax(0,2fr)_minmax(0,1fr)] items-center gap-6 py-4 px-6 hover:bg-gray-50 cursor-pointer transition-colors"
								onClick={() => (window.location.href = `/${patient.name}`)}
							>
								<div className="flex items-center gap-3 min-w-0">
									<span className="h-10 w-10 rounded-full bg-gray-100 flex items-center justify-center shrink-0">
										{patient.icon}
									</span>
									<div className="min-w-0 flex-1">
										<div className="text-sm font-medium text-gray-900 truncate">
											{patient.name}
										</div>
										<div className="text-xs text-gray-600 truncate">
											{getStatusText(patient.status)}
										</div>
									</div>
								</div>

								<div className="min-w-0 flex-1">
									<a className="block text-sm font-medium text-blue-600 hover:text-blue-700 hover:underline truncate">
										{getActionText(getStatusText(patient.status))}
									</a>
									<div className="mt-1 text-xs text-gray-600 flex items-center gap-2">
										<time>{formatDate(patient.date)}</time>
										{patient.unit && (
											<>
												<span>en</span>
												<span className="inline-flex items-center gap-1">
													<GitBranch className="h-3.5 w-3.5" />
													{patient.unit}
												</span>
											</>
										)}
									</div>
								</div>

								<div className="flex items-center justify-end gap-2 shrink-0 min-w-0">
									{patient.hasGithub ? (
										<a
											className="inline-flex items-center h-7 px-2.5 rounded-full border border-gray-200 bg-gray-50 text-gray-700 text-xs min-w-0 max-w-[120px] truncate"
											title="Severidad y signos"
										>
											<Github className="h-3.5 w-3.5 mr-1.5 shrink-0" />
											<span className="truncate">{patient.github}</span>
										</a>
									) : (
										<div className="w-[120px]"></div>
									)}
									<button
										className="h-8 w-8 rounded-full text-gray-400 hover:bg-gray-50 shrink-0 flex items-center justify-center"
										title="Lista de acciones"
									>
										<Activity className="h-4 w-4" />
									</button>
									<DropdownMenu>
										<DropdownMenuTrigger asChild>
											<button
												className="h-8 w-8 rounded-full text-gray-600 hover:bg-gray-50 shrink-0 flex items-center justify-center"
												title="Más"
											>
												<MoreHorizontal className="h-4 w-4" />
											</button>
										</DropdownMenuTrigger>
										<DropdownMenuContent align="end">
											<DropdownMenuItem>Abrir</DropdownMenuItem>
											<DropdownMenuItem>Ver notas</DropdownMenuItem>
											<DropdownMenuItem>
												{getActionText(getStatusText(patient.status))}
											</DropdownMenuItem>
										</DropdownMenuContent>
									</DropdownMenu>
								</div>
							</li>
						))
					) : (
						<li className="text-center py-8 text-gray-500">
							No se encontraron pacientes.{" "}
							<a href="#" className="text-blue-600 hover:underline">
								Cambiar filtros
							</a>
						</li>
					)}
				</ul>
			</div>
		</div>
	);
};
