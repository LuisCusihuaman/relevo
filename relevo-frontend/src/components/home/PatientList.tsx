import type { FC } from "react";
import { projects } from "@/pages/data";

export const PatientList: FC = () => {
	const getStatusText = (status: string): string => {
		switch (status) {
			case "Connect Git Repository":
				return "Iniciar traspaso";
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

	return (
		<div className="p-4">
			<h3 className="text-sm font-medium text-gray-500 px-4 mb-3">
				Pacientes
			</h3>
			<div className="space-y-2">
				{projects.map((project, index) => (
					<button
						key={index}
						className="flex items-center gap-3 w-full px-4 py-3 text-left hover:bg-gray-50 rounded-lg"
					>
						<div className="h-8 w-8 rounded-full bg-gray-200 flex items-center justify-center text-sm font-semibold">
							{project.icon}
						</div>
						<div className="flex-1 min-w-0">
							<div className="text-base font-medium text-gray-900 truncate">
								{project.name}
							</div>
							<div className="text-sm text-gray-600 truncate">
								{getStatusText(project.status)}
							</div>
						</div>
					</button>
				))}
			</div>
		</div>
	);
};
