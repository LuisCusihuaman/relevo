import { type FC } from "react";
import { GitBranch } from "lucide-react";

type DeploymentsMobileListProps = {
	handleDeploymentClick: (deploymentId: string, projectName: string) => void;
};

export const DeploymentsMobileList: FC<DeploymentsMobileListProps> = ({
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
							<h3 className="font-medium text-gray-900 text-base hover:underline cursor-pointer">
								{deployment.id}
							</h3>
							<p className="text-sm text-gray-500">
								{deployment.environmentType}
							</p>
						</div>
						{deployment.current && (
							<span className="bg-blue-100 text-blue-800 text-xs px-2 py-1 rounded-full font-medium">
								Current
							</span>
						)}
					</div>

					{/* Status */}
					<div className="flex items-center gap-2">
						<span
							className={`h-2 w-2 rounded-full ${deployment.statusColor}`}
						></span>
						<span className="text-sm font-medium text-gray-900">
							{deployment.status}
						</span>
						<span className="text-sm text-gray-500">
							{deployment.statusTime}
						</span>
					</div>

					{/* Environment */}
					<div>
						<div
							className={`text-sm font-medium ${deployment.environmentColor}`}
						>
							{deployment.environment}
						</div>
						<div className="text-sm text-gray-500">
							{deployment.time}
						</div>
					</div>

					{/* Project Info */}
					<div className="flex items-center gap-2">
						<div
							className={`w-6 h-6 rounded-full flex items-center justify-center text-sm ${deployment.projectIcon.bg} ${deployment.projectIcon.text || "text-gray-700"}`}
						>
							{deployment.projectIcon.value}
						</div>
						<span className="font-medium text-gray-900 text-sm hover:underline cursor-pointer">
							{deployment.project}
						</span>
					</div>

					{/* Source Info */}
					<div className="space-y-1">
						<div className="flex items-center gap-1 text-sm text-gray-600">
							<GitBranch className="h-4 w-4" />
							<span>{deployment.branch}</span>
						</div>
						<div className="flex items-center gap-2">
							<span className="font-mono text-sm bg-gray-100 px-2 py-1 rounded text-gray-700">
								{deployment.commit}
							</span>
							<span className="text-sm text-gray-600">
								{deployment.message}
							</span>
						</div>
					</div>

					{/* Author */}
					<div className="flex items-center justify-between pt-2 border-t border-gray-100">
						<div className="flex items-center gap-2">
							<div className="w-6 h-6 rounded-full bg-blue-500 flex items-center justify-center text-xs font-medium text-white">
								{deployment.avatar}
							</div>
							<span className="text-sm text-gray-600">
								{deployment.time} by {deployment.author}
							</span>
						</div>
					</div>
				</div>
			))}
		</div>
	);
};
