import { CheckCircle, Calendar, Clock } from "lucide-react";
import type { ShiftConfig, ShiftCheckInStep } from "../types";
import type { ReactElement } from "react";

type ShiftSelectionStepProps = {
	currentStep: ShiftCheckInStep;
	selectedShift: string;
	shifts: Array<ShiftConfig>;
	isEditing: boolean;
	onShiftSelect: (shiftId: string) => void;
	translation: (key: string, options?: Record<string, unknown>) => string;
};

export function ShiftSelectionStep({
	currentStep,
	selectedShift,
	shifts,
	isEditing,
	onShiftSelect,
	translation: t,
}: ShiftSelectionStepProps): ReactElement {
	if (currentStep !== 2) return <></>;

	return (
		<div className="space-y-6">
			<div className="text-center space-y-2">
				<h3 className="text-xl font-semibold text-foreground">
					{isEditing ? t("updateYourShift") : t("selectYourShift")}
				</h3>
				<p className="text-muted-foreground">
					{isEditing ? t("changeShiftAssignment") : t("whenProvidingCare")}
				</p>
			</div>

			<div className="space-y-4">
				<div className="flex items-center gap-3 mb-4">
					<div className="w-8 h-8 bg-primary/10 rounded-lg flex items-center justify-center">
						<Clock className="w-5 h-5 text-primary" />
					</div>
					<h3 className="font-medium text-foreground">{t("availableShifts")}</h3>
				</div>

				<div className="space-y-3 mobile-scroll-fix">
					{shifts.map((shiftOption) => (
						<button
							key={shiftOption.id}
							className={`w-full p-4 rounded-xl border-2 transition-all text-left medical-card-hover ${
								selectedShift === shiftOption.id
									? "border-primary bg-primary/5"
									: "border-border hover:border-border/80 hover:bg-muted/50"
							}`}
											onClick={() => {
												onShiftSelect(shiftOption.id);
											}}
						>
							<div className="flex items-center gap-3">
								<div
									className={`w-10 h-10 rounded-lg flex items-center justify-center ${
										selectedShift === shiftOption.id ? "bg-primary/10" : "bg-muted/50"
									}`}
								>
									<Calendar
										className={`w-5 h-5 ${
											selectedShift === shiftOption.id ? "text-primary" : "text-muted-foreground"
										}`}
									/>
								</div>
								<div className="flex-1">
									<div className="font-medium text-foreground">{shiftOption.name}</div>
									<div className="text-sm text-muted-foreground">{shiftOption.time}</div>
								</div>
								{selectedShift === shiftOption.id && (
									<CheckCircle className="w-5 h-5 text-primary" />
								)}
							</div>
						</button>
					))}
				</div>
			</div>
		</div>
	);
}
