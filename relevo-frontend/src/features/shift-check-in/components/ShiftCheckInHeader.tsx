import type { ShiftCheckInStep } from "../types";
import type { ReactElement } from "react";
import { MobileHeader } from "./MobileHeader";
import { DesktopHeader } from "./DesktopHeader";

type ShiftCheckInHeaderProps = {
	currentStep: ShiftCheckInStep;
	doctorName: string;
	totalSteps: number;
	onSignOut: () => Promise<void>;
	getStepTitle: (step: ShiftCheckInStep) => string;
};

export function ShiftCheckInHeader({
	currentStep,
	doctorName,
	totalSteps,
	onSignOut,
	getStepTitle,
}: ShiftCheckInHeaderProps): ReactElement {
	return (
		<>
			<div className="md:hidden">
				<MobileHeader
					currentStep={currentStep}
					totalSteps={totalSteps}
					onSignOut={onSignOut}
				/>
			</div>
			<div className="hidden md:block">
				<DesktopHeader
					currentStep={currentStep}
					doctorName={doctorName}
					getStepTitle={getStepTitle}
					totalSteps={totalSteps}
					onSignOut={onSignOut}
				/>
			</div>
		</>
	);
}
