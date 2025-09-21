import { memo, type ReactElement } from "react";
import { useTranslation } from "react-i18next";

import { Card, CardContent } from "@/components/ui/card";

import { DoctorInfoStep } from "./DoctorInfoStep";
import { PatientSelectionStep } from "./PatientSelectionStep";
import { SetupHeader } from "./SetupHeader";
import { SetupNavigation } from "./SetupNavigation";
import { ShiftSelectionStep } from "./ShiftSelectionStep";
import { UnitSelectionStep } from "./UnitSelectionStep";
import { useSetupState } from "../hooks/useSetupState";
import type { ShiftConfig, UnitConfig } from "../types";

type SetupWizardProps = {
	units: Array<UnitConfig>;
	shifts: Array<ShiftConfig>;
};

const TOTAL_STEPS = 4;

function SetupWizardComponent({ units, shifts }: SetupWizardProps): ReactElement {
	const { t } = useTranslation(["dailySetup", "handover", "patientSelectionCard"]);

	const {
		currentStep,
		isMobile,
		doctorName,
		unit,
		shift,
		selectedIndexes,
		showValidationError,
		isEditing,
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
	} = useSetupState();

	const getStepTitle = (step: number): string => {
		switch (step) {
			case 0:
				return isEditing
					? (t("stepTitle.updateInfo") as string) || "Update Info"
					: (t("stepTitle.yourInfo") as string) || "Your Info";
			case 1:
				return isEditing
					? (t("stepTitle.updateUnit") as string) || "Update Unit"
					: (t("stepTitle.unitSelection") as string) || "Unit Selection";
			case 2:
				return isEditing
					? (t("stepTitle.updateShift") as string) || "Update Shift"
					: (t("stepTitle.shiftSelection") as string) || "Shift Selection";
			case 3:
				return isEditing
					? (t("stepTitle.updatePatients") as string) || "Update Patients"
					: (t("stepTitle.patientSelection") as string) || "Patient Selection";
			default:
				return (t("stepTitle.setup") as string) || "Setup";
		}
	};

	const canProceed = canProceedToNextStep();

	const renderStepContent = (): ReactElement => {
		switch (currentStep) {
			case 0:
				return (
					<DoctorInfoStep
						doctorName={doctorName}
						isEditing={isEditing}
						translation={t}
					/>
				);
			case 1:
				return (
					<UnitSelectionStep
						currentStep={currentStep}
						doctorName={doctorName}
						isEditing={isEditing}
						selectedUnit={unit}
						translation={t}
						units={units}
						onUnitSelect={setUnit}
					/>
				);
			case 2:
				return (
					<ShiftSelectionStep
						currentStep={currentStep}
						isEditing={isEditing}
						selectedShift={shift}
						shifts={shifts}
						translation={t}
						onShiftSelect={setShift}
					/>
				);
			case 3:
				return (
					<PatientSelectionStep
						currentStep={currentStep}
						isEditing={isEditing}
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

	if (isMobile) {
		return (
			<div
				className="bg-background flex flex-col"
				style={{
					height: "100dvh",
					maxHeight: "100dvh",
				}}
			>
				<SetupHeader
					currentStep={currentStep}
					doctorName={doctorName}
					getStepTitle={getStepTitle}
					isEditing={isEditing}
					isMobile={isMobile}
					totalSteps={TOTAL_STEPS}
					translation={t}
					onSignOut={handleSignOut}
				/>

				<div className="flex-1 flex flex-col min-h-0">
					<div className="flex-1 overflow-y-auto mobile-scroll-fix">
						<div className="p-4 pt-28 pb-32">{renderStepContent()}</div>
					</div>
				</div>

				<SetupNavigation
					canProceed={canProceed}
					currentStep={currentStep}
					isEditing={isEditing}
					isMobile={isMobile}
					totalSteps={TOTAL_STEPS}
					translation={t}
					onBack={handleBackStep}
					onNext={() => {
						handleNextStep(patients);
					}}
				/>

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

	return (
		<div className="min-h-screen bg-background flex items-center justify-center p-6">
			<Card className="w-full max-w-4xl bg-white shadow-sm border border-border">
				<SetupHeader
					currentStep={currentStep}
					doctorName={doctorName}
					getStepTitle={getStepTitle}
					isEditing={isEditing}
					isMobile={isMobile}
					totalSteps={TOTAL_STEPS}
					translation={t}
					onSignOut={handleSignOut}
				/>

				<CardContent className="space-y-6 pt-28">
					{renderStepContent()}

					<SetupNavigation
						canProceed={canProceed}
						currentStep={currentStep}
						isEditing={isEditing}
						isMobile={isMobile}
						totalSteps={TOTAL_STEPS}
						translation={t}
						onBack={handleBackStep}
						onNext={() => {
							handleNextStep(patients);
						}}
					/>
				</CardContent>
			</Card>
		</div>
	);
}

export const SetupWizard = memo(SetupWizardComponent);
