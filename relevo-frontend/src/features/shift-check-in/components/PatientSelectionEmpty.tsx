import type { ReactElement } from "react";
import { useTranslation } from "react-i18next";

export function PatientSelectionEmpty(): ReactElement {
	const { t } = useTranslation(["dailySetup", "handover"]);

	return (
		<div className="flex flex-col h-full">
			<div className="flex-shrink-0 space-y-6">
				<div className="text-center space-y-3">
					<h3 className="text-xl font-semibold text-foreground">
						{t("selectYourPatients")}
					</h3>
					<div className="flex items-center justify-center py-8">
						<div className="text-muted-foreground">
							{t("noPatients") || "No patients available for this unit"}
						</div>
					</div>
				</div>
			</div>
		</div>
	);
}
