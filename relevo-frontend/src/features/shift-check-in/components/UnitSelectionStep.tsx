import { CheckCircle, MapPin } from "lucide-react";
import type { UnitConfig } from "@/types/domain";
import type { ShiftCheckInStep } from "../types";
import type { ReactElement } from "react";

type UnitSelectionStepProps = {
	currentStep: ShiftCheckInStep;
	doctorName: string;
	selectedUnit: string;
	units: Array<UnitConfig>;
	isEditing: boolean;
	onUnitSelect: (unitId: string) => void;
	translation: (key: string, options?: Record<string, unknown>) => string;
};

function getUnitIcon(unitId: string): typeof MapPin {
	switch (unitId) {
		case "picu":
			return MapPin; // Pediatric Intensive Care - Heart for critical care
		case "nicu":
			return MapPin; // Neonatal ICU - Baby icon
		case "general":
			return MapPin; // General Pediatrics - Classic medical icon
		case "cardiology":
			return MapPin; // Cardiology - Heart activity/EKG
		case "surgery":
			return MapPin; // Surgery - Surgical scissors
		default:
			return MapPin; // Fallback
	}
}

export function UnitSelectionStep({
	currentStep,
	doctorName,
	selectedUnit,
	units,
	isEditing,
	onUnitSelect,
	translation: t,
}: UnitSelectionStepProps): ReactElement {
	if (currentStep !== 1) return <></>;

	return (
		<div className="space-y-6">
			<div className="text-center space-y-2">
				<h2 className="text-xl font-semibold text-foreground">
					{t("greeting", { doctorName })}
				</h2>
				<p className="text-muted-foreground">
					{isEditing ? t("updateUnitAssignment") : t("configureShiftDetails")}
				</p>
			</div>

			<div className="space-y-4">
				<div className="flex items-center gap-3 mb-4">
					<div className="w-8 h-8 bg-primary/10 rounded-lg flex items-center justify-center">
						<MapPin className="w-5 h-5 text-primary" />
					</div>
					<h3 className="font-medium text-foreground">
						{isEditing ? t("changeYourUnit") : t("selectYourUnit")}
					</h3>
				</div>

				<div className="space-y-3 mobile-scroll-fix">
					{units.map((unitOption) => {
						const UnitIcon = getUnitIcon(unitOption.id);
						return (
							<button
								key={unitOption.id}
								className={`w-full p-4 rounded-xl border-2 transition-all text-left medical-card-hover ${
									selectedUnit === unitOption.id
										? "border-primary bg-primary/5"
										: "border-border hover:border-border/80 hover:bg-muted/50"
								}`}
								onClick={() => {
									onUnitSelect(unitOption.id);
								}}
							>
								<div className="flex items-center gap-3">
									<div
										className={`w-10 h-10 rounded-lg flex items-center justify-center ${
											selectedUnit === unitOption.id ? "bg-primary/10" : "bg-muted/50"
										}`}
									>
										<UnitIcon
											className={`w-5 h-5 ${
												selectedUnit === unitOption.id ? "text-primary" : "text-muted-foreground"
											}`}
										/>
									</div>
									<div className="flex-1">
										<div className="font-medium text-foreground">{unitOption.name}</div>
										<div className="text-sm text-muted-foreground">{unitOption.description}</div>
									</div>
									{selectedUnit === unitOption.id && (
										<CheckCircle className="w-5 h-5 text-primary" />
									)}
								</div>
							</button>
						);
					})}
				</div>
			</div>
		</div>
	);
}
