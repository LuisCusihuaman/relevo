import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { AlertCircle, CheckCircle, Circle } from "lucide-react";
import { PatientSelectionCard } from "@/components/PatientSelectionCard";
import type { SetupPatient, SetupStep } from "../types";
import type { ReactElement } from "react";

type PatientSelectionStepProps = {
	currentStep: SetupStep;
	selectedIndexes: Array<number>;
	patients: Array<SetupPatient>;
	isEditing: boolean;
	showValidationError: boolean;
	onPatientToggle: (index: number) => void;
	onSelectAll: () => void;
	translation: (key: string, options?: Record<string, unknown>) => string;
};

export function PatientSelectionStep({
	currentStep,
	selectedIndexes,
	patients,
	isEditing,
	showValidationError,
	onPatientToggle,
	onSelectAll,
	translation: t,
	isFetching = false,
}: PatientSelectionStepProps & { isFetching?: boolean }): ReactElement {
	if (currentStep !== 3) return <></>;

	// Show loading state if no patients yet or currently fetching
	if (patients.length === 0 && isFetching) {
		return (
			<div className="flex flex-col h-full">
				<div className="flex-shrink-0 space-y-6">
					<div className="text-center space-y-3">
						<h3 className="text-xl font-semibold text-foreground">
							{isEditing ? t("updateYourPatients") : t("selectYourPatients")}
						</h3>
						<div className="flex items-center justify-center py-8">
							<div className="flex items-center gap-2 text-muted-foreground">
								<div className="w-4 h-4 border-2 border-current border-t-transparent rounded-full animate-spin" />
								<span>{t("loadingPatients") || "Loading patients..."}</span>
							</div>
						</div>
					</div>
				</div>
			</div>
		);
	}

	// Show empty state if no patients available
	if (patients.length === 0 && !isFetching) {
		return (
			<div className="flex flex-col h-full">
				<div className="flex-shrink-0 space-y-6">
					<div className="text-center space-y-3">
						<h3 className="text-xl font-semibold text-foreground">
							{isEditing ? t("updateYourPatients") : t("selectYourPatients")}
						</h3>
						<div className="flex items-center justify-center py-8">
							<div className="text-muted-foreground">{t("noPatients") || "No patients available for this unit"}</div>
						</div>
					</div>
				</div>
			</div>
		);
	}

	return (
		<div className="flex flex-col h-full">
			<div className="flex-shrink-0 space-y-6">
				<div className="text-center space-y-3">
					<h3 className="text-xl font-semibold text-foreground">
						{isEditing ? t("updateYourPatients") : t("selectYourPatients")}
					</h3>

					<Badge
						variant="outline"
						className={`text-base px-4 py-2 ${
							selectedIndexes.length > 0
								? "bg-primary/10 border-primary/30 text-primary"
								: "bg-muted/30 border-border/50 text-muted-foreground"
						}`}
					>
						{t("patientsSelected", {
							count: selectedIndexes.length,
							total: patients.length,
						})}
					</Badge>

					{isEditing && (
						<p className="text-sm text-muted-foreground">
							{t("previouslyAssigned", { count: 0 })}
						</p>
					)}
				</div>

				<div className="flex items-center justify-end">
					<Button
						className="gap-2"
						size="sm"
						variant="outline"
						onClick={onSelectAll}
					>
						{selectedIndexes.length === patients.length ? (
							<>
								<Circle className="w-4 h-4" />
								{t("deselectAll")}
							</>
						) : (
							<>
								<CheckCircle className="w-4 h-4" />
								{t("selectAll")}
							</>
						)}
					</Button>
				</div>

				{showValidationError && (
					<div className="p-4 rounded-lg bg-red-50 border border-red-200 text-red-700 flex items-center gap-3">
						<AlertCircle className="w-5 h-5 flex-shrink-0" />
						<div>
							<p className="font-medium text-sm">{t("validationErrorTitle")}</p>
							<p className="text-sm">{t("validationErrorBody")}</p>
						</div>
					</div>
				)}
			</div>

			<div className="flex-1 min-h-0 mt-6">
				<div className="h-full overflow-y-auto mobile-scroll-fix">
					<div className="space-y-3 pb-4">
						{patients.map((patient, index) => (
							<div
								key={patient.id}
								className="cursor-pointer"
								role="button"
								tabIndex={0}
								onClick={() => {
									onPatientToggle(index);
								}}
								onKeyDown={(event) => {
									if (event.key === "Enter" || event.key === " ") {
										onPatientToggle(index);
									}
								}}
							>
								<PatientSelectionCard
									isSelected={selectedIndexes.includes(index)}
									patient={patient}
									translation={t}
								/>
							</div>
						))}
					</div>
				</div>
			</div>
		</div>
	);
}
