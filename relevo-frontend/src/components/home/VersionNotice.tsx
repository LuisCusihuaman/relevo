import type { FC } from "react";
import { AlertTriangle } from "lucide-react";

export const VersionNotice: FC = () => {
	return (
		<div className="bg-orange-50 border border-orange-200 rounded-lg p-4">
			<div className="flex items-start gap-3">
				<AlertTriangle className="h-5 w-5 text-orange-600 mt-0.5 flex-shrink-0" />
				<div className="flex-1">
					<p className="text-sm text-orange-800">
						<strong>
							Tienes 5 pacientes sin traspaso activo.
						</strong>{" "}
						Debes iniciar sus traspasos pronto.
					</p>
				</div>
			</div>
		</div>
	);
};
