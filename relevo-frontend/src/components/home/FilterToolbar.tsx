import type { FC } from "react";
import { Calendar } from "lucide-react";

export const FilterToolbar: FC = () => {
	return (
		<div className="flex items-center justify-between mt-6 mb-4">
			<div className="flex items-center gap-4">
				<button className="flex items-center gap-2 px-3 py-2 border border-gray-300 rounded-lg text-sm hover:bg-gray-50">
					<Calendar className="h-4 w-4" />
					Rango de fechas
				</button>
			</div>
			<div className="flex items-center gap-4">
				<select className="px-3 py-2 border border-gray-300 rounded-lg text-sm bg-white">
					<option>Todas las unidades</option>
					<option>UCI</option>
					<option>Emergencia</option>
					<option>Pediatría</option>
					<option>Cardiología</option>
				</select>
				<select className="px-3 py-2 border border-gray-300 rounded-lg text-sm bg-white">
					<option>Estado</option>
					<option>Todos</option>
					<option>No iniciado</option>
					<option>En progreso</option>
					<option>Completado</option>
					<option>Fallido</option>
				</select>
			</div>
		</div>
	);
};
