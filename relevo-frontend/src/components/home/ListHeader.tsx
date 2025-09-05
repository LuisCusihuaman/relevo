import type { FC } from "react";

export const ListHeader: FC = () => {
	return (
		<div className="flex items-center justify-between">
			<div>
				<h1 className="text-2xl font-semibold text-gray-900">
					Deployments
				</h1>
				<p className="text-sm text-gray-600 mt-1">
					All deployments from{" "}
					<code className="bg-gray-100 px-1 py-0.5 rounded text-xs">
						luis-cusihuamans-projects
					</code>
				</p>
			</div>
		</div>
	);
};
