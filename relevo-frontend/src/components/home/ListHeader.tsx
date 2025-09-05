import type { FC } from "react";

export const ListHeader: FC = () => {
	return (
		<div className="flex items-center justify-between">
			<div>
				<h1 className="text-2xl font-semibold text-gray-900">Traspasos</h1>
				<p className="text-sm text-gray-600 mt-1">
					Bandeja de sesiones I-PASS
				</p>
			</div>
		</div>
	);
};
