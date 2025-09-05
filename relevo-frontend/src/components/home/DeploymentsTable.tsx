import { type FC } from "react";
import { GitBranch } from "lucide-react";

type DeploymentsTableProps = {
	handleDeploymentClick: (deploymentId: string, projectName: string) => void;
};

export const DeploymentsTable: FC<DeploymentsTableProps> = ({
	handleDeploymentClick,
}) => {
	const deployments = [
		{
			id: "CDMFQKRs",
			status: "Error",
			statusColor: "bg-red-500",
			environment: "Unexpected Error",
			environmentColor: "text-red-600",
			project: "calendar-app",
			projectIcon: {
				type: "emoji",
				value: "❄️",
				bg: "bg-blue-100",
			},
			branch: "dependabot/npm_and_yarn",
			commit: "c5b235d",
			message: "chore(deps): bump dependencies",
			time: "2d ago",
			author: "dependabot[bot]",
			statusTime: "2s (2d ago)",
			environmentType: "Preview",
			avatar: "DB",
		},
		{
			id: "8LB4tSUAh",
			status: "Error",
			statusColor: "bg-red-500",
			environment: "Unexpected Error",
			environmentColor: "text-red-600",
			project: "calendar-app",
			projectIcon: {
				type: "emoji",
				value: "❄️",
				bg: "bg-blue-100",
			},
			branch: "dependabot/npm_and_yarn",
			commit: "7d4dbb5",
			message: "chore(deps): bump dependencies",
			time: "3d ago",
			author: "dependabot[bot]",
			statusTime: "2s (3d ago)",
			environmentType: "Preview",
			avatar: "DB",
		},
		{
			id: "3L5k6ngCp",
			status: "Error",
			statusColor: "bg-red-500",
			environment: "Unexpected Error",
			environmentColor: "text-red-600",
			project: "heroes-app",
			projectIcon: {
				type: "emoji",
				value: "❄️",
				bg: "bg-blue-100",
			},
			branch: "dependabot/npm_and_yarn",
			commit: "7a50c77",
			message: "chore(deps): bump dependencies",
			time: "Aug 6",
			author: "dependabot[bot]",
			statusTime: "4s (18d ago)",
			environmentType: "Preview",
			avatar: "DB",
		},
		{
			id: "GX6A8fhaZ",
			status: "Error",
			statusColor: "bg-red-500",
			environment: "Unexpected Error",
			environmentColor: "text-red-600",
			project: "calendar-app",
			projectIcon: {
				type: "emoji",
				value: "❄️",
				bg: "bg-blue-100",
			},
			branch: "dependabot/npm_and_yarn",
			commit: "1348002",
			message: "chore(deps): bump dependencies",
			time: "Jul 17",
			author: "dependabot[bot]",
			statusTime: "4s (38d ago)",
			environmentType: "Preview",
			avatar: "DB",
		},
		{
			id: "6qYUWvuN3",
			status: "Ready",
			statusColor: "bg-green-500",
			environment: "Promoted",
			environmentColor: "text-gray-600",
			project: "relevo-app",
			projectIcon: {
				type: "text",
				value: "V",
				bg: "bg-purple-500",
				text: "text-white",
			},
			branch: "main",
			commit: "Current",
			message: "Production Rebuild of 8Td...",
			time: "Jul 12",
			author: "luiscusihuaman",
			statusTime: "29s (43d ago)",
			current: true,
			environmentType: "Production",
			avatar: "LC",
		},
		{
			id: "8TdfXLHgY",
			status: "Ready",
			statusColor: "bg-green-500",
			environment: "Staged",
			environmentColor: "text-gray-600",
			project: "relevo-app",
			projectIcon: {
				type: "text",
				value: "V",
				bg: "bg-purple-500",
				text: "text-white",
			},
			branch: "eduardoc/spanish",
			commit: "e78260a",
			message: "Enhance diabetes management...",
			time: "Jul 12",
			author: "luiscusihuaman",
			statusTime: "21s (43d ago)",
			environmentType: "Preview",
			avatar: "LC",
		},
	];
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
