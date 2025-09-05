import type { FC } from "react";
import {
	DropdownMenu,
	DropdownMenuContent,
	DropdownMenuItem,
	DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Activity, GitBranch, Github, MoreHorizontal } from "lucide-react";
import type { Project } from "./types";

type ProjectsListProps = {
	projects: Array<Project>;
};

export const ProjectsList: FC<ProjectsListProps> = ({ projects }) => {
	return (
		<div className="flex-1 min-w-0">
			<div className="mb-4">
				<h2 className="text-base font-medium leading-tight">
					Projects
				</h2>
			</div>
			<div className="bg-white border border-gray-200 rounded-xl shadow-sm overflow-hidden">
				<ul className="divide-y divide-gray-200">
					{projects.map((project, index) => (
						<li
							key={index}
							className="grid grid-cols-[minmax(0,2fr)_minmax(0,2fr)_minmax(0,1fr)] items-center gap-6 py-4 px-6 hover:bg-gray-50 cursor-pointer transition-colors"
							onClick={() =>
								(window.location.href = `/${project.name}`)
							}
						>
							{/* Col 1 — Project */}
							<div className="flex items-center gap-3 min-w-0">
								<span className="h-10 w-10 rounded-full bg-gray-100 flex items-center justify-center shrink-0">
									{project.name === "relevo-app" ||
									project.name === "psa-frontend" ? (
										<svg
											fill="#6B7280"
											height="20"
											viewBox="0 0 75 65"
											width="20"
										>
											<path d="M37.59.25l36.95 64H.64l36.95-64z" />
										</svg>
									) : project.name === "calendar-app" ||
									  project.name === "heroes-app" ? (
										<svg
											fill="#6B7280"
											height="20"
											viewBox="0 0 24 24"
											width="20"
										>
											<circle cx="12" cy="12" r="10" />
											<path
												d="M8 12h8"
												stroke="currentColor"
												strokeWidth="2"
											/>
											<path
												d="M12 8v8"
												stroke="currentColor"
												strokeWidth="2"
											/>
										</svg>
									) : project.name ===
									  "v0-portfolio-template-by-v0" ? (
										<div className="w-5 h-5 bg-gray-400 rounded-full"></div>
									) : project.name === "image-component" ||
									  project.name === "v0-music-game-concept" ? (
										<div className="w-6 h-6 bg-gray-600 rounded-full flex items-center justify-center">
											<span className="text-white text-xs font-bold">
												N
											</span>
										</div>
									) : (
										<svg
											fill="#6B7280"
											height="20"
											viewBox="0 0 75 65"
											width="20"
										>
											<path d="M37.59.25l36.95 64H.64l36.95-64z" />
										</svg>
									)}
								</span>
								<div className="min-w-0 flex-1">
									<div className="text-sm font-medium text-gray-900 truncate">
										{project.name}
									</div>
									<div className="text-xs text-gray-600 truncate">
										{project.url}
									</div>
								</div>
							</div>

							{/* Col 2 — Activity */}
							<div className="min-w-0 flex-1">
								{project.status === "Connect Git Repository" ? (
									<a className="block text-sm font-medium text-blue-600 hover:text-blue-700 hover:underline truncate">
										{project.status}
									</a>
								) : (
									<div className="block text-sm font-medium text-gray-900 truncate">
										{project.status}
									</div>
								)}
								<div className="mt-1 text-xs text-gray-600 flex items-center gap-2">
									<time>{project.date}</time>
									{project.branch && (
										<>
											<span>on</span>
											<span className="inline-flex items-center gap-1">
												<GitBranch className="h-3.5 w-3.5" />
												{project.branch}
											</span>
										</>
									)}
								</div>
							</div>

							{/* Col 3 — Repo chip + Health + ⋯ */}
							<div className="flex items-center justify-end gap-2 shrink-0 min-w-0">
								{project.hasGithub ? (
									<a className="inline-flex items-center h-7 px-2.5 rounded-full border border-gray-200 bg-gray-50 text-gray-700 text-xs min-w-0 max-w-[120px] truncate">
										<Github className="h-3.5 w-3.5 mr-1.5 shrink-0" />
										<span className="truncate">
											{project.github}
										</span>
									</a>
								) : (
									<div className="w-[120px]"></div>
								)}
								<button className="h-8 w-8 rounded-full text-gray-400 hover:bg-gray-50 shrink-0 flex items-center justify-center">
									<Activity className="h-4 w-4" />
								</button>
								<DropdownMenu>
									<DropdownMenuTrigger asChild>
										<button className="h-8 w-8 rounded-full text-gray-600 hover:bg-gray-50 shrink-0 flex items-center justify-center">
											<MoreHorizontal className="h-4 w-4" />
										</button>
									</DropdownMenuTrigger>
									<DropdownMenuContent align="end">
										<DropdownMenuItem>
											View Project
										</DropdownMenuItem>
										<DropdownMenuItem>Settings</DropdownMenuItem>
										<DropdownMenuItem className="text-red-600">
											Delete
										</DropdownMenuItem>
									</DropdownMenuContent>
								</DropdownMenu>
							</div>
						</li>
					))}
				</ul>
			</div>
		</div>
	);
};
