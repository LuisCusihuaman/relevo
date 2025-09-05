import type { FC } from "react";
import { GitBranch } from "lucide-react";
import { deployments } from "../../pages/data";

type EntityTableProps = {
	handleDeploymentClick: (deploymentId: string, projectName: string) => void;
};

export const EntityTable: FC<EntityTableProps> = ({
	handleDeploymentClick,
}) => {
	return (
		<div className="hidden md:block rounded-lg border border-gray-200 bg-white overflow-hidden">
			{deployments.map((deployment, index) => (
				<div
					key={deployment.id}
					className={`grid grid-cols-[1fr_1fr_1fr_1fr_1fr] items-center gap-4 py-3 px-4 hover:bg-gray-50 transition-colors cursor-pointer ${
						index < 5 ? "border-b border-gray-100" : ""
					}`}
					onClick={() => {
						handleDeploymentClick(deployment.id, deployment.project);
					}}
				>
					{/* Deployment Column */}
					<div className="min-w-0">
						<div className="font-medium text-gray-900 text-sm hover:underline cursor-pointer truncate">
							{deployment.id}
						</div>
						<div className="text-xs text-gray-500 mt-0.5">
							{deployment.environmentType}
						</div>
					</div>

					{/* Status Column */}
					<div className="min-w-0">
						<div className="flex items-center gap-2 mb-1">
							<span
								className={`h-2 w-2 rounded-full ${deployment.statusColor}`}
							></span>
							<span className="text-sm font-medium text-gray-900">
								{deployment.status}
							</span>
						</div>
						<div className="text-xs text-gray-500">
							{deployment.statusTime}
						</div>
					</div>

					{/* Environment Column */}
					<div className="min-w-0">
						<div
							className={`text-sm ${deployment.environmentColor} font-medium`}
						>
							{deployment.environment}
						</div>
						<div className="text-xs text-gray-500 mt-0.5">
							{deployment.time}
						</div>
					</div>

					{/* Source Column */}
					<div className="min-w-0">
						<div className="flex items-center gap-2 mb-1">
							<div
								className={`w-5 h-5 rounded-full flex items-center justify-center text-xs ${deployment.projectIcon.bg} ${deployment.projectIcon.text || "text-gray-700"}`}
							>
								{deployment.projectIcon.value}
							</div>
							<span className="font-medium text-gray-900 text-sm hover:underline cursor-pointer truncate">
								{deployment.project}
							</span>
						</div>
						<div className="flex items-center gap-1 text-xs text-gray-500">
							<GitBranch className="h-3 w-3" />
							<span className="truncate">{deployment.branch}</span>
						</div>
						<div className="flex items-center gap-1 mt-0.5">
							<span className="font-mono text-xs bg-gray-100 px-1 py-0.5 rounded text-gray-700">
								{deployment.commit}
							</span>
							<span className="text-xs text-gray-500 truncate">
								{deployment.message}
							</span>
						</div>
					</div>

					{/* Created Column */}
					<div className="min-w-0 text-right">
						<div className="flex items-center justify-end gap-2">
							<GitBranch className="h-3 w-3 text-gray-400" />
							<span className="text-xs text-gray-500">
								{deployment.time} by {deployment.author}
							</span>
							<div className="w-6 h-6 rounded-full bg-blue-500 flex items-center justify-center text-xs font-medium text-white">
								{deployment.avatar}
							</div>
						</div>
					</div>
				</div>
			))}
		</div>
	);
};
