import { type FC } from "react";
import { RotateCcw } from "lucide-react";
import { type Project } from "./types";

type ProjectHeaderProps = {
	currentProject: Project;
};

export const ProjectHeader: FC<ProjectHeaderProps> = ({ currentProject }) => {
	return (
		<div>
			<h1 className="text-2xl font-semibold text-gray-900 mb-8">
				{currentProject.name}
			</h1>

			<div className="bg-white border border-gray-200 rounded-xl p-6">
				<div className="flex items-center justify-between mb-6">
					<h2 className="text-lg font-medium">
						Production Deployment
					</h2>
					<div className="flex items-center gap-3">
						<button className="px-3 py-1.5 text-sm border border-gray-200 rounded-md hover:bg-gray-50">
							Build Logs
						</button>
						<button className="px-3 py-1.5 text-sm border border-gray-200 rounded-md hover:bg-gray-50">
							Runtime Logs
						</button>
						<button className="px-3 py-1.5 text-sm border border-gray-200 rounded-md hover:bg-gray-50 flex items-center gap-2">
							<RotateCcw className="h-4 w-4" />
							Instant Rollback
						</button>
					</div>
				</div>

				<div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
					<div className="bg-gray-50 rounded-lg p-4 flex items-center justify-center h-64">
						<div className="text-center">
							<div className="w-16 h-16 bg-purple-100 rounded-lg flex items-center justify-center mx-auto mb-3">
								<div className="w-8 h-8 bg-purple-600 rounded flex items-center justify-center">
									<span className="text-white text-sm font-bold">
										V
									</span>
								</div>
							</div>
							<p className="text-sm text-gray-600">
								Configuración de RELEVO
							</p>
						</div>
					</div>

					<div className="space-y-4">
						<div>
							<h3 className="text-sm font-medium text-gray-700 mb-1">
								Deployment
							</h3>
							<p className="text-sm text-gray-900">
								{currentProject?.name}
								-1a70w6d3y-luis-cusihuamans-projects.vercel.app
							</p>
						</div>
					</div>
				</div>
			</div>
		</div>
	);
};
