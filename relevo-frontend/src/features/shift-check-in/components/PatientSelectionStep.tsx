import type { ShiftCheckInPatient } from "@/types/domain";
import type { ShiftCheckInStep } from "../types";
import type { ReactElement } from "react";

import { PatientSelectionLoading } from "./PatientSelectionLoading";
import { PatientSelectionEmpty } from "./PatientSelectionEmpty";
import { PatientSelectionList } from "./PatientSelectionList";

type PatientSelectionStepProps = {
	currentStep: ShiftCheckInStep;
	selectedIndexes: Array<number>;
	patients: Array<ShiftCheckInPatient>;
	showValidationError: boolean;
	onPatientToggle: (index: number) => void;
	onSelectAll: () => void;
};

export function PatientSelectionStep({
	currentStep,
	selectedIndexes,
	patients,
	showValidationError,
	onPatientToggle,
	onSelectAll,
	isFetching = false,
}: PatientSelectionStepProps & { isFetching?: boolean }): ReactElement {
	if (currentStep !== 3) return <></>;

	// Show loading state if no patients yet or currently fetching
	if (patients.length === 0 && isFetching) {
		return <PatientSelectionLoading />;
	}

	// Show empty state if no patients available
	if (patients.length === 0 && !isFetching) {
		return <PatientSelectionEmpty />;
	}

	return (
		<PatientSelectionList
			patients={patients}
			selectedIndexes={selectedIndexes}
			showValidationError={showValidationError}
			onPatientToggle={onPatientToggle}
			onSelectAll={onSelectAll}
		/>
	);
}
