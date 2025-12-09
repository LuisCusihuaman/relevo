import { memo, type ReactElement } from "react";
import { useTranslation } from "react-i18next";

import { Card } from "@/components/ui/card";
import type { ShiftConfig, UnitConfig } from "@/types/domain";

import { DoctorInfoStep } from "./DoctorInfoStep";
import { PatientSelectionStep } from "./PatientSelectionStep";
import { ShiftCheckInHeader } from "./ShiftCheckInHeader";
import { ShiftCheckInNavigation } from "./ShiftCheckInNavigation";
import { ShiftSelectionStep } from "./ShiftSelectionStep";
import { UnitSelectionStep } from "./UnitSelectionStep";
import { useShiftCheckInState } from "../hooks/useShiftCheckInState";

type ShiftCheckInWizardProps = {
	units: Array<UnitConfig>;
	shifts: Array<ShiftConfig>;
};

const TOTAL_STEPS = 4;

function ShiftCheckInWizardComponent({ units, shifts }: ShiftCheckInWizardProps): ReactElement {
	const { t } = useTranslation(["dailySetup", "handover", "patientSelectionCard"]);

	const {
		currentStep,
		doctorName,
		unit,
		shift,
		selectedIndexes,
		showValidationError,
		patients,
		isFetching,
		setUnit,
		setShift,
		togglePatientSelection,
		handleSelectAll,
		canProceedToNextStep,
		handleNextStep,
		handleBackStep,
		handleSignOut,
	} = useShiftCheckInState();

	const getStepTitle = (step: number): string => {
		switch (step) {
			case 0:
				return (t("stepTitle.yourInfo")) || "Your Info";
			case 1:
				return (t("stepTitle.unitSelection")) || "Unit Selection";
			case 2:
				return (t("stepTitle.shiftSelection")) || "Shift Selection";
			case 3:
				return (t("stepTitle.patientSelection")) || "Patient Selection";
			default:
				return (t("stepTitle.setup")) || "Setup";
		}
	};

	const canProceed = canProceedToNextStep();

	const renderStepContent = (): ReactElement => {
		switch (currentStep) {
			case 0:
				return (
					<DoctorInfoStep
						doctorName={doctorName}
					/>
				);
			case 1:
				return (
					<UnitSelectionStep
						currentStep={currentStep}
						doctorName={doctorName}
						selectedUnit={unit}
						units={units}
						onUnitSelect={setUnit}
					/>
				);
			case 2:
				return (
					<ShiftSelectionStep
						currentStep={currentStep}
						selectedShift={shift}
						shifts={shifts}
						onShiftSelect={setShift}
					/>
				);
			case 3:
				return (
					<PatientSelectionStep
						currentStep={currentStep}
						isFetching={isFetching}
						patients={patients}
						selectedIndexes={selectedIndexes}
						showValidationError={showValidationError}
						onPatientToggle={togglePatientSelection}
						onSelectAll={() => {
							handleSelectAll(patients);
						}}
					/>
				);
			default:
				return <></>;
		}
	};

	return (
		<div className="min-h-[100dvh] bg-background flex flex-col md:items-center md:justify-center md:p-6">
			<ShiftCheckInNavigation
				canProceed={canProceed}
				currentStep={currentStep}
				totalSteps={TOTAL_STEPS}
				onBack={handleBackStep}
				onNext={() => {
					handleNextStep(patients);
				}}
			/>

			<Card className="w-full h-[100dvh] md:h-auto md:max-w-4xl bg-background md:bg-white shadow-none md:shadow-sm border-0 md:border md:border-border flex flex-col">
				<ShiftCheckInHeader
					currentStep={currentStep}
					doctorName={doctorName}
					getStepTitle={getStepTitle}
					totalSteps={TOTAL_STEPS}
					onSignOut={handleSignOut}
				/>

				<div className="flex-1 overflow-y-auto p-4 pt-28 pb-32 md:p-6 md:pt-28 mobile-scroll-fix space-y-6">
					{renderStepContent()}
				</div>
			</Card>

			{currentStep === 3 && selectedIndexes.length === 0 && (
				<div
					className="fixed bottom-0 left-0 right-0 z-20 bg-red-50 border-t border-red-200 px-4 py-2"
					style={{
						paddingBottom: "max(env(safe-area-inset-bottom), 12px)",
						marginBottom: "96px",
					}}
				>
					<p className="text-xs text-red-700 text-center">{t("mobileValidation")}</p>
				</div>
			)}
		</div>
	);
}

export const ShiftCheckInWizard = memo(ShiftCheckInWizardComponent);
