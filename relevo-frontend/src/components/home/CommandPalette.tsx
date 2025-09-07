import type { Dispatch, FC, SetStateAction } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Search } from "lucide-react";

import type { SearchResult } from "./types";

type CommandPaletteProps = {
	setIsSearchOpen: (isOpen: boolean) => void;
	searchQuery: string;
	setSearchQuery: Dispatch<SetStateAction<string>>;
	searchResults: Array<SearchResult>;
};

export const CommandPalette: FC<CommandPaletteProps> = ({
	setIsSearchOpen,
	searchQuery,
	setSearchQuery,
	searchResults,
}) => {
	const getCategoryText = (category: string): string => {
		switch (category) {
			case "Projects":
			case "Project":
				return "Pacientes";
			case "Deployments":
			case "Latest Deployment":
				return "Traspasos";
			case "Team":
				return "Equipo";
			case "Actions":
			case "Navigation Assistant":
				return "Acciones rápidas";
			case "Notas clínicas":
				return "Notas clínicas";
			case "Unidades/Servicios":
				return "Unidades/Servicios";
			case "Personas/Equipo":
				return "Personas/Equipo";
			default:
				return category;
		}
	};

	const filteredResults = searchResults.filter(
		(result) =>
			searchQuery === "" ||
			result.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
			result.category.toLowerCase().includes(searchQuery.toLowerCase())
	);

	return (
		<div
			className="fixed inset-0 z-[9999] bg-white/20 backdrop-blur-sm"
			onClick={() => {
				setIsSearchOpen(false);
			}}
		>
			<div
				className="absolute top-16 right-4 md:right-48 bg-white rounded-lg shadow-xl w-96 border border-gray-200"
				onClick={(event_) => {
					event_.stopPropagation();
				}}
			>
				{/* Functional search input header */}
				<div className="border-b border-gray-100 p-4">
					<div className="relative">
						<Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-gray-400" />
						<Input
							autoFocus
							className="pl-10 pr-16 h-10 border-gray-300 focus:border-gray-400 focus:ring-0"
							placeholder="Buscar… (F)"
							value={searchQuery}
							onChange={(event_) => {
								setSearchQuery(event_.target.value);
							}}
						/>
						<Button
							className="absolute right-2 top-1/2 transform -translate-y-1/2 text-xs text-gray-400 hover:text-gray-600"
							size="sm"
							variant="ghost"
							onClick={() => {
								setIsSearchOpen(false);
							}}
						>
							Esc
						</Button>
					</div>
				</div>

				{/* Search Results */}
				<div className="p-2 max-h-96 overflow-y-auto">
					{filteredResults.length > 0 ? (
						filteredResults.map((result, index) => (
							<div
								key={index}
								className="flex items-center gap-3 p-3 rounded-md hover:bg-gray-50 cursor-pointer"
							>
								<div className="flex-shrink-0">
									{result.type === "handover" && (
										<div className="w-2 h-2 bg-green-500 rounded-full"></div>
									)}
									{result.type === "team" && (
										<img
											alt="Team"
											className="w-5 h-5 rounded-full"
											src="/placeholder.svg?height=20&width=20"
										/>
									)}
									{result.type === "project" &&
										result.name === "relevo-app" && (
											<div className="w-5 h-5 bg-purple-500 rounded flex items-center justify-center">
												<span className="text-white text-sm font-bold">
													V
												</span>
											</div>
										)}
									{result.type === "project" &&
										result.name === "Analytics" && (
											<div className="w-5 h-5 flex items-center justify-center">
												<svg
													className="w-4 h-4 text-gray-600"
													fill="none"
													stroke="currentColor"
													viewBox="0 0 24 24"
												>
													<path
														d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z"
														strokeLinecap="round"
														strokeLinejoin="round"
														strokeWidth={2}
													/>
												</svg>
											</div>
										)}
									{result.type === "assistant" && (
										<div className="w-5 h-5 flex items-center justify-center">
											<svg
												className="w-4 h-4 text-gray-600"
												fill="none"
												stroke="currentColor"
												viewBox="0 0 24 24"
											>
												<path
													d="M13 10V3L4 14h7v7l9-11h-7z"
													strokeLinecap="round"
													strokeLinejoin="round"
													strokeWidth={2}
												/>
											</svg>
										</div>
									)}
								</div>
								<div className="flex-1 min-w-0">
									<div className="font-medium text-gray-900 truncate">
										{result.name}
									</div>
									<div className="text-sm text-gray-500 truncate">
										{getCategoryText(result.category)}
									</div>
								</div>
							</div>
						))
					) : (
						<div className="p-6 text-center">
							<div className="text-sm font-medium text-gray-700">Sin resultados</div>
							<div className="mt-1 text-xs text-gray-500">Prueba con el nombre del paciente o escribe ‘acción’ para ver comandos</div>
						</div>
					)}
				</div>
			</div>
		</div>
	);
};

