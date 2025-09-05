import type { FC } from "react";
import { Calendar } from "lucide-react";

export const DeploymentsToolbar: FC = () => {
	return (
		<div className="flex items-center justify-between mt-6 mb-4">
			<div className="flex items-center gap-4">
				<button className="flex items-center gap-2 px-3 py-2 border border-gray-300 rounded-lg text-sm hover:bg-gray-50">
					<Calendar className="h-4 w-4" />
					Select Date Range
				</button>
			</div>
			<div className="flex items-center gap-4">
				<select className="px-3 py-2 border border-gray-300 rounded-lg text-sm bg-white">
					<option>All Environments</option>
					<option>Production</option>
					<option>Preview</option>
				</select>
				<select className="px-3 py-2 border border-gray-300 rounded-lg text-sm bg-white">
					<option>Status 5/6</option>
					<option>Ready</option>
					<option>Error</option>
				</select>
			</div>
		</div>
	);
};
