import type { FC } from "react";
import { Button } from "@/components/ui/button";
import { ChevronDown } from "lucide-react";
import {
	Popover,
	PopoverContent,
	PopoverTrigger,
} from "@/components/ui/popover";

type Metric = {
	label: string;
	value: string;
	tooltip: string;
	currentValue: string;
	totalValue: string;
};

const metrics: Array<Metric> = [
	{
		label: "Pacientes asignados",
		value: "12",
		tooltip: "Pacientes asignados a ti en el turno actual.",
		currentValue: "12",
		totalValue: "15",
	},
	{
		label: "Traspasos en progreso",
		value: "5",
		tooltip: "Sesiones de traspaso abiertas sin completar.",
		currentValue: "5",
		totalValue: "10",
	},
	{
		label: "Acciones pendientes",
		value: "18",
		tooltip: "Tareas con vencimiento hoy.",
		currentValue: "18",
		totalValue: "25",
	},
	{
		label: "Alertas críticas",
		value: "2",
		tooltip: "Severidad Crítico en últimas 24 h.",
		currentValue: "2",
		totalValue: "3",
	},
];

export const SectionMeta: FC = () => {
	return (
		<div>
			<h2 className="text-base font-medium mb-4 leading-tight">
				Indicadores del turno
			</h2>
			<div className="border border-gray-200 rounded-lg bg-white">
				<div className="p-6">
					<div className="flex items-center justify-between mb-6">
						<div>
							<p className="text-base font-medium text-gray-900 leading-tight">
								Últimos 30 días
							</p>
							<p className="text-sm text-gray-600 mt-1 leading-tight">
								Actualizado hace 2 min
							</p>
						</div>
						<Button
							className="bg-black text-white hover:bg-gray-800 h-8 px-3 text-sm font-medium"
							size="sm"
						>
							Ver más
						</Button>
					</div>

					<div className="space-y-3">
						{metrics.map((metric) => (
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
												{metric.label}
											</span>
										</div>
										<span className="text-sm text-gray-600 font-mono leading-tight">
											{metric.value}
										</span>
									</div>
								</PopoverTrigger>
								<PopoverContent>
									<p className="text-sm">{metric.tooltip}</p>
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
