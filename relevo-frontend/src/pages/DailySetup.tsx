import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
	Activity,
	AlertCircle,
	Baby,
	Building2,
	Calendar,
	CheckCircle,
	ChevronLeft,
	ChevronRight,
	Circle,
	Clock,
	Heart,
	MapPin,
	Scissors,
	Stethoscope,
	User,
} from "lucide-react";
import { useEffect, useState, type ReactElement } from "react";
import { useTranslation } from "react-i18next";
import { useNavigate } from "@tanstack/react-router";

import type { UnitConfig, ShiftConfig, SetupPatient } from "@/common/types";
import { formatDiagnosis } from "@/lib/formatters";
import { PatientSelectionCard } from "@/components/PatientSelectionCard";
import {
	useUnitsQuery,
	useShiftsQuery,
	usePatientsByUnitQuery,
	useAssignPatientsMutation,
	type ApiUnit,
	type ApiShift,
	type ApiPatient,
} from "@/api/daily-setup";

export function DailySetup(): ReactElement {
	const { t } = useTranslation(["dailySetup", "handover"]);
	const navigate = useNavigate();
	const isEditing = false;
	const existingSetup = null as unknown as {
		doctorName?: string;
		unit?: string;
		shift?: string;
		selectedPatients?: Array<number>;
	} | null;
	const [currentStep, setCurrentStep] = useState(0);
	const [isMobile, setIsMobile] = useState(false);

	// Use state initialized from props for editing
	const [doctorName, setDoctorName] = useState(existingSetup?.doctorName || "");
	const [unit, setUnit] = useState(existingSetup?.unit || "");
	const [shift, setShift] = useState(existingSetup?.shift || "");
	const [selectedIndexes, setSelectedIndexes] = useState<Array<number>>([]);
	const [showValidationError, setShowValidationError] = useState(false);

	// Fetch from API with ES fallbacks
	const unitsQuery = useUnitsQuery();
	const shiftsQuery = useShiftsQuery();
	const patientsQuery = usePatientsByUnitQuery(unit || undefined);
	const assignMutation = useAssignPatientsMutation();

	const apiUnits: Array<ApiUnit> | undefined = unitsQuery.data;
	const apiShifts: Array<ApiShift> | undefined = shiftsQuery.data;
	const apiPatients: Array<ApiPatient> | undefined = patientsQuery.data;

	const currentUnitsConfig: Array<UnitConfig> = (apiUnits ?? []).map((u) => ({
		id: u.id,
		name: u.name,
		description: u.description ?? "",
	}));

	const currentShiftsConfig: Array<ShiftConfig> = (apiShifts ?? []).map(
		(s) => ({
			id: s.id,
			name: s.name,
			time: s.startTime && s.endTime ? `${s.startTime} - ${s.endTime}` : "",
		})
	);

	const toStatus = (s?: string): "pending" | "in-progress" | "complete" =>
		s === "pending" || s === "in-progress" || s === "complete" ? s : "pending";

	const toSeverity = (v?: string): "stable" | "watcher" | "unstable" =>
		v === "stable" || v === "watcher" || v === "unstable" ? v : "watcher";

	const currentPatientsSource: Array<SetupPatient> = (apiPatients ?? []).map(
		(p) => ({
			id: Number(p.id),
			name: p.name,
			age: p.age,
			room: p.room ?? "",
			diagnosis: p.diagnosis ? formatDiagnosis(p.diagnosis) : "",
			status: toStatus(p.status),
			severity: toSeverity(p.severity),
		})
	);

	const currentPatients = currentPatientsSource.map((p: SetupPatient) => ({
		id: p.id,
		name: p.name,
		age: p.age,
		room: p.room,
		diagnosis: formatDiagnosis(p.diagnosis),
		status: p.status,
		severity: p.severity,
	}));

	// Helper function to get medical icons for different units
	const getUnitIcon = (unitId: string): typeof Heart => {
		switch (unitId) {
			case "picu":
				return Heart; // Pediatric Intensive Care - Heart for critical care
			case "nicu":
				return Baby; // Neonatal ICU - Baby icon
			case "general":
				return Stethoscope; // General Pediatrics - Classic medical icon
			case "cardiology":
				return Activity; // Cardiology - Heart activity/EKG
			case "surgery":
				return Scissors; // Surgery - Surgical scissors
			default:
				return Building2; // Fallback
		}
	};

	useEffect((): (() => void) => {
		const checkIsMobile = (): void => {
			setIsMobile(window.innerWidth < 768);
		};
		checkIsMobile();
		window.addEventListener("resize", checkIsMobile);
		return () => {
			window.removeEventListener("resize", checkIsMobile);
		};
	}, []);

	const handlePatientToggle = (rowIndex: number): void => {
		setSelectedIndexes((previous: Array<number>) =>
			previous.includes(rowIndex)
				? previous.filter((index: number) => index !== rowIndex)
				: [...previous, rowIndex]
		);
		if (showValidationError) setShowValidationError(false);
	};

	const handleSelectAll = (): void => {
		if (selectedIndexes.length === currentPatients.length) {
			setSelectedIndexes([]);
		} else {
			setSelectedIndexes(Array.from({ length: currentPatients.length }, (_, index) => index));
		}
		if (showValidationError) setShowValidationError(false);
	};

	const canProceedToNextStep = (): boolean => {
		switch (currentStep) {
			case 0:
				return doctorName.trim() !== "";
			case 1:
				return unit !== "";
			case 2:
				return shift !== "";
			case 3:
				return selectedIndexes.length > 0;
			default:
				return false;
		}
	};

	const handleNextStep = (): void => {
		if (currentStep === 3 && selectedIndexes.length === 0) {
			setShowValidationError(true);
			return;
		}

		if (canProceedToNextStep()) {
			if (currentStep === 3) {
				const shiftId = shift; // shift is guaranteed by canProceedToNextStep
				const selected = selectedIndexes.map((index) => currentPatients[index]?.id).filter(Boolean) as Array<string | number>;
				const payload = { shiftId, patientIds: selected.map(String) };
				assignMutation.mutate(payload, {
					onSettled: () => {
						window.localStorage.setItem("dailySetupCompleted", "true");
						void navigate({ to: "/" });
					},
				});
			} else {
				setCurrentStep((previous: number) => previous + 1);
			}
		}
	};

	const handleBackStep = (): void => {
		if (currentStep > 0) {
			setCurrentStep((previous: number) => previous - 1);
			setShowValidationError(false);
		}
	};

	const renderStepContent = (): ReactElement | null => {
		switch (currentStep) {
			case 0:
				return (
					<div className="space-y-6">
						{/* Clean Professional Header - Modified for editing */}
						<div className="text-center space-y-4">
							<div className="w-16 h-16 bg-white border border-border rounded-xl flex items-center justify-center mx-auto shadow-sm">
								<div className="w-10 h-10 bg-primary rounded-lg flex items-center justify-center">
									<span className="text-primary-foreground font-semibold text-lg">
										R
									</span>
								</div>
							</div>

							<div className="space-y-2">
								<h1 className="text-2xl font-semibold text-foreground">
									{isEditing ? t("updateYourSetup") : t("welcome")}
								</h1>
								<p className="text-muted-foreground">
									{isEditing
										? t("modifyAssignments")
										: t("platformDescription")}
								</p>
								{!isEditing && (
									<div className="flex items-center justify-center gap-4 text-sm text-muted-foreground pt-1">
										<span>{t("ipassProtocol")}</span>
										<span>•</span>
										<span>{t("secureDocumentation")}</span>
									</div>
								)}
							</div>
						</div>

						{/* Clean Professional Name Input */}
						<div className="space-y-4">
							<div className="space-y-2">
								<Label
									className="text-base font-medium flex items-center gap-2"
									htmlFor="doctorName"
								>
									<User className="w-4 h-4 text-primary" />
									{t("yourNameLabel")}
								</Label>
								<Input
									className="h-12 text-base border-border focus:border-primary bg-white"
									id="doctorName"
									placeholder={t("namePlaceholder")}
									type="text"
									value={doctorName}
									onChange={(event_) => {
										setDoctorName(event_.target.value);
									}}
								/>
								<p className="text-sm text-muted-foreground">
									{isEditing ? t("updateNameHelp") : t("nameHelp")}
								</p>
							</div>
						</div>
					</div>
				);

			case 1:
				return (
					<div className="space-y-6">
						{/* Clean Step Header */}
						<div className="text-center space-y-2">
							<h2 className="text-xl font-semibold text-foreground">
								{t("greeting", { doctorName })}
							</h2>
							<p className="text-muted-foreground">
								{isEditing
									? t("updateUnitAssignment")
									: t("configureShiftDetails")}
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

							{/* MOBILE SCROLLABLE UNIT LIST */}
							<div className="space-y-3 mobile-scroll-fix">
								{currentUnitsConfig.map((unitOption) => {
									const UnitIcon = getUnitIcon(unitOption.id);
									return (
										<button
											key={unitOption.id}
											className={`w-full p-4 rounded-xl border-2 transition-all text-left medical-card-hover ${
												unit === unitOption.id
													? "border-primary bg-primary/5"
													: "border-border hover:border-border/80 hover:bg-muted/50"
											}`}
											onClick={() => {
												setUnit(unitOption.id);
											}}
										>
											<div className="flex items-center gap-3">
												<div
													className={`w-10 h-10 rounded-lg flex items-center justify-center ${
														unit === unitOption.id
															? "bg-primary/10"
															: "bg-muted/50"
													}`}
												>
													<UnitIcon
														className={`w-5 h-5 ${unit === unitOption.id ? "text-primary" : "text-muted-foreground"}`}
													/>
												</div>
												<div className="flex-1">
													<div className="font-medium text-foreground">
														{unitOption.name}
													</div>
													<div className="text-sm text-muted-foreground">
														{unitOption.description}
													</div>
												</div>
												{unit === unitOption.id && (
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

			case 2:
				return (
					<div className="space-y-6">
						<div className="text-center space-y-2">
							<h3 className="text-xl font-semibold text-foreground">
								{isEditing ? t("updateYourShift") : t("selectYourShift")}
							</h3>
							<p className="text-muted-foreground">
								{isEditing
									? t("changeShiftAssignment")
									: t("whenProvidingCare")}
							</p>
						</div>

						<div className="space-y-4">
							<div className="flex items-center gap-3 mb-4">
								<div className="w-8 h-8 bg-primary/10 rounded-lg flex items-center justify-center">
									<Clock className="w-5 h-5 text-primary" />
								</div>
								<h3 className="font-medium text-foreground">
									{t("availableShifts")}
								</h3>
							</div>

							{/* MOBILE SCROLLABLE SHIFT LIST */}
							<div className="space-y-3 mobile-scroll-fix">
								{currentShiftsConfig.map((shiftOption) => (
									<button
										key={shiftOption.id}
										className={`w-full p-4 rounded-xl border-2 transition-all text-left medical-card-hover ${
											shift === shiftOption.id
												? "border-primary bg-primary/5"
												: "border-border hover:border-border/80 hover:bg-muted/50"
										}`}
										onClick={() => {
											setShift(shiftOption.id);
										}}
									>
										<div className="flex items-center gap-3">
											<div
												className={`w-10 h-10 rounded-lg flex items-center justify-center ${
													shift === shiftOption.id
														? "bg-primary/10"
														: "bg-muted/50"
												}`}
											>
												<Calendar
													className={`w-5 h-5 ${shift === shiftOption.id ? "text-primary" : "text-muted-foreground"}`}
												/>
											</div>
											<div className="flex-1">
												<div className="font-medium text-foreground">
													{shiftOption.name}
												</div>
												<div className="text-sm text-muted-foreground">
													{shiftOption.time}
												</div>
											</div>
											{shift === shiftOption.id && (
												<CheckCircle className="w-5 h-5 text-primary" />
											)}
										</div>
									</button>
								))}
							</div>
						</div>
					</div>
				);

			case 3:
				return (
					<div className="flex flex-col h-full">
						{/* SIMPLIFIED HEADER - ESSENTIAL INFO ONLY */}
						<div className="flex-shrink-0 space-y-6">
							<div className="text-center space-y-3">
								<h3 className="text-xl font-semibold text-foreground">
									{isEditing
										? t("updateYourPatients")
										: t("selectYourPatients")}
								</h3>

								{/* PROMINENT COUNTER */}
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
										total: currentPatients.length,
									})}
								</Badge>

								{/* NEW: Show current selection status for editing */}
								{isEditing && existingSetup?.selectedPatients && (
									<p className="text-sm text-muted-foreground">
										{t("previouslyAssigned", {
											count: existingSetup.selectedPatients.length,
										})}
									</p>
								)}
							</div>

							{/* SIMPLIFIED CONTROLS */}
							<div className="flex items-center justify-end">
								<Button
									className="gap-2"
									size="sm"
									variant="outline"
									onClick={handleSelectAll}
								>
									{selectedIndexes.length === currentPatients.length ? (
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

							{/* VALIDATION ERROR */}
							{showValidationError && (
								<div className="p-4 rounded-lg bg-red-50 border border-red-200 text-red-700 flex items-center gap-3">
									<AlertCircle className="w-5 h-5 flex-shrink-0" />
									<div>
										<p className="font-medium text-sm">
											{t("validationErrorTitle")}
										</p>
										<p className="text-sm">{t("validationErrorBody")}</p>
									</div>
								</div>
							)}
						</div>

						{/* PATIENT LIST */}
						<div className="flex-1 min-h-0 mt-6">
							<div className="h-full overflow-y-auto mobile-scroll-fix">
								<div className="space-y-3 pb-4">
									{currentPatients.map((patient, index) => (
										<div
											key={patient.id}
											className="cursor-pointer"
											role="button"
											tabIndex={0}
											onClick={() => { handlePatientToggle(index); }}
											onKeyDown={(event_) => {
												if (event_.key === "Enter" || event_.key === " ") {
													handlePatientToggle(index);
												}
											}}
										>
											<PatientSelectionCard
												isSelected={selectedIndexes.includes(index)}
												patient={patient as unknown as SetupPatient}
											/>
										</div>
									))}
								</div>
							</div>
						</div>
					</div>
				);

			default:
				return null;
		}
	};

	const getStepTitle = (): string => {
		switch (currentStep) {
			case 0:
				return isEditing ? t("stepTitle.updateInfo") : t("stepTitle.yourInfo");
			case 1:
				return isEditing
					? t("stepTitle.updateUnit")
					: t("stepTitle.unitSelection");
			case 2:
				return isEditing
					? t("stepTitle.updateShift")
					: t("stepTitle.shiftSelection");
			case 3:
				return isEditing
					? t("stepTitle.updatePatients")
					: t("stepTitle.patientSelection");
			default:
				return t("stepTitle.setup");
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
				{/* Mobile Header - With Safe Area */}
				<div
					className="flex-shrink-0 p-4 bg-white border-b border-border"
					style={{
						paddingTop: "max(env(safe-area-inset-top), 16px)",
					}}
				>
					<div className="flex items-center justify-between mb-4">
						<div>
							<h1 className="font-semibold text-primary">RELEVO</h1>
							<p className="text-xs text-muted-foreground">
								{isEditing ? t("mobileHeader.update") : t("mobileHeader.new")}
							</p>
						</div>
						<div className="text-sm text-muted-foreground">
							{t("mobileHeader.step", { current: currentStep + 1, total: 4 })}
						</div>
					</div>

					{/* Clean Progress Bar */}
					<div className="flex gap-2">
						{[0, 1, 2, 3].map((step) => (
							<div
								key={step}
								className={`h-2 flex-1 rounded-full transition-colors ${
									step <= currentStep ? "bg-primary" : "bg-muted"
								}`}
							/>
						))}
					</div>
				</div>

				{/* Content - ENHANCED MOBILE SCROLLING WITH PROPER BOTTOM SPACING */}
				<div className="flex-1 flex flex-col min-h-0">
					<div className="flex-1 overflow-y-auto mobile-scroll-fix">
						<div className="p-4 pb-32">{renderStepContent()}</div>
					</div>
				</div>

				{/* Floating Action Buttons - Like Main App Bottom Nav */}
				<div
					className="fixed bottom-0 left-0 right-0 z-30"
					style={{
						paddingBottom: "max(env(safe-area-inset-bottom), 12px)",
					}}
				>
					<div className="bg-background/95 backdrop-blur-md mx-3 mb-3 rounded-2xl px-3 py-4 border border-border/40 shadow-lg">
						<div className="flex items-center gap-3">
							{/* Back Button - Show on all steps except first */}
							{currentStep > 0 && (
								<Button
									className="gap-2 h-12 px-6 rounded-xl"
									size="lg"
									variant="outline"
									onClick={handleBackStep}
								>
									<ChevronLeft className="w-4 h-4" />
									{t("back")}
								</Button>
							)}

							{/* Next/Complete Button - Takes remaining space */}
							<Button
								className="flex-1 gap-2 h-12 rounded-xl"
								disabled={!canProceedToNextStep()}
								size="lg"
								onClick={handleNextStep}
							>
								{currentStep === 3
									? isEditing
										? t("saveChanges")
										: t("startUsingRelevo")
									: t("continue")}
								<ChevronRight className="w-4 h-4" />
							</Button>
						</div>
					</div>
				</div>

				{/* Validation Help Text */}
				{currentStep === 3 && selectedIndexes.length === 0 && (
					<div
						className="fixed bottom-0 left-0 right-0 z-20 bg-red-50 border-t border-red-200 px-4 py-2"
						style={{
							paddingBottom: "max(env(safe-area-inset-bottom), 12px)",
							marginBottom: "96px", // Space for floating buttons
						}}
					>
						<p className="text-xs text-red-700 text-center">
							{t("mobileValidation")}
						</p>
					</div>
				)}
			</div>
		);
	}

	// Enhanced Desktop Layout
	return (
		<div className="min-h-screen bg-background flex items-center justify-center p-6">
			<Card className="w-full max-w-4xl bg-white shadow-sm border border-border">
				<CardHeader className="text-center space-y-4">
					<div className="flex items-center justify-center gap-4">
						<div className="w-12 h-12 bg-white border border-border rounded-xl flex items-center justify-center shadow-sm">
							<div className="w-8 h-8 bg-primary rounded-lg flex items-center justify-center">
								<span className="text-primary-foreground font-semibold">R</span>
							</div>
						</div>
						<div>
							<CardTitle className="text-2xl text-foreground">
								{isEditing ? t("desktopHeader.update") : t("desktopHeader.new")}
							</CardTitle>
							<p className="text-muted-foreground">
								{currentStep === 0
									? isEditing
										? t("desktopSubheader.update")
										: t("desktopSubheader.new")
									: t("desktopSubheader.progress", {
											action: isEditing ? t("update") : t("configure"),
											name: doctorName,
										})}
							</p>
						</div>
					</div>

					<div className="flex items-center justify-between">
						<Badge className="text-primary" variant="outline">
							{getStepTitle()}
						</Badge>
						<div className="text-sm text-muted-foreground">
							{t("mobileHeader.step", { current: currentStep + 1, total: 4 })}
						</div>
					</div>

					{/* Clean Progress Bar */}
					<div className="flex gap-2">
						{[0, 1, 2, 3].map((step) => (
							<div
								key={step}
								className={`h-2 flex-1 rounded-full transition-colors ${
									step <= currentStep ? "bg-primary" : "bg-muted"
								}`}
							/>
						))}
					</div>
				</CardHeader>

				<CardContent className="space-y-6">
					{/* Compact Desktop Unit/Shift Selection */}
					{currentStep === 1 ? (
						<div className="space-y-6">
							<div className="text-center space-y-2">
								<h2 className="text-xl font-semibold text-foreground">
									{t("greeting", { doctorName })}
								</h2>
								<p className="text-muted-foreground">
									{isEditing
										? t("updateUnitAssignment")
										: t("configureShiftDetails")}
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

								{/* Compact Grid Layout for Units */}
								<div className="grid grid-cols-1 lg:grid-cols-2 gap-3">
									{currentUnitsConfig.map((unitOption) => {
										const UnitIcon = getUnitIcon(unitOption.id);
										return (
											<button
												key={unitOption.id}
												className={`p-3 rounded-lg border-2 transition-all text-left ${
													unit === unitOption.id
														? "border-primary bg-primary/5"
														: "border-border hover:border-border/80 hover:bg-muted/50"
												}`}
												onClick={() => {
													setUnit(unitOption.id);
												}}
											>
												<div className="flex items-center gap-3">
													<div
														className={`w-8 h-8 rounded-lg flex items-center justify-center ${
															unit === unitOption.id
																? "bg-primary/10"
																: "bg-muted/50"
														}`}
													>
														<UnitIcon
															className={`w-4 h-4 ${unit === unitOption.id ? "text-primary" : "text-muted-foreground"}`}
														/>
													</div>
													<div className="flex-1 min-w-0">
														<div className="font-medium text-foreground text-sm">
															{unitOption.name}
														</div>
														<div className="text-xs text-muted-foreground truncate">
															{unitOption.description}
														</div>
													</div>
													{unit === unitOption.id && (
														<CheckCircle className="w-4 h-4 text-primary flex-shrink-0" />
													)}
												</div>
											</button>
										);
									})}
								</div>
							</div>
						</div>
					) : currentStep === 2 ? (
						<div className="space-y-6">
							<div className="text-center space-y-2">
								<h3 className="text-xl font-semibold text-foreground">
									{isEditing ? t("updateYourShift") : t("selectYourShift")}
								</h3>
								<p className="text-muted-foreground">
									{isEditing
										? t("changeShiftAssignment")
										: t("whenProvidingCare")}
								</p>
							</div>

							<div className="space-y-4">
								<div className="flex items-center gap-3 mb-4">
									<div className="w-8 h-8 bg-primary/10 rounded-lg flex items-center justify-center">
										<Clock className="w-5 h-5 text-primary" />
									</div>
									<h3 className="font-medium text-foreground">
										{t("availableShifts")}
									</h3>
								</div>

								{/* Compact Grid Layout for Shifts */}
								<div className="grid grid-cols-1 lg:grid-cols-3 gap-3">
									{currentShiftsConfig.map((shiftOption) => (
										<button
											key={shiftOption.id}
											className={`p-3 rounded-lg border-2 transition-all text-left ${
												shift === shiftOption.id
													? "border-primary bg-primary/5"
													: "border-border hover:border-border/80 hover:bg-muted/50"
											}`}
											onClick={() => {
												setShift(shiftOption.id);
											}}
										>
											<div className="flex items-center gap-3">
												<div
													className={`w-8 h-8 rounded-lg flex items-center justify-center ${
														shift === shiftOption.id
															? "bg-primary/10"
															: "bg-muted/50"
													}`}
												>
													<Calendar
														className={`w-4 h-4 ${shift === shiftOption.id ? "text-primary" : "text-muted-foreground"}`}
													/>
												</div>
												<div className="flex-1 min-w-0">
													<div className="font-medium text-foreground text-sm">
														{shiftOption.name}
													</div>
													<div className="text-xs text-muted-foreground">
														{shiftOption.time}
													</div>
												</div>
												{shift === shiftOption.id && (
													<CheckCircle className="w-4 h-4 text-primary flex-shrink-0" />
												)}
											</div>
										</button>
									))}
								</div>
							</div>
						</div>
					) : currentStep === 3 ? (
						/* SIMPLIFIED DESKTOP PATIENT SELECTION */
						<div className="space-y-6">
							<div className="text-center space-y-3">
								<h3 className="text-xl font-semibold text-foreground">
									{isEditing
										? t("updateYourPatients")
										: t("selectYourPatients")}
								</h3>

								{/* PROMINENT COUNTER - DESKTOP */}
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
										total: currentPatients.length,
									})}
								</Badge>

								{/* NEW: Show current selection status for editing */}
								{isEditing && existingSetup?.selectedPatients && (
									<p className="text-sm text-muted-foreground">
										{t("previouslyAssigned", {
											count: existingSetup.selectedPatients.length,
										})}
									</p>
								)}
							</div>

							<div className="flex items-center justify-end">
								<Button
									className="gap-2"
									size="sm"
									variant="outline"
									onClick={handleSelectAll}
								>
									{selectedIndexes.length === currentPatients.length ? (
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

							{/* Validation Error */}
							{showValidationError && (
								<div className="p-4 rounded-lg bg-red-50 border border-red-200 text-red-700 flex items-center gap-3">
									<AlertCircle className="w-5 h-5 flex-shrink-0" />
									<div>
										<p className="font-medium text-sm">
											{t("validationErrorTitle")}
										</p>
										<p className="text-sm">{t("validationErrorBody")}</p>
									</div>
								</div>
							)}

							{/* PATIENT GRID */}
							<div className="relative">
								<div className="patient-scroll-container bg-muted/10 border border-border/40 rounded-xl p-4">
									<div className="max-h-[380px] overflow-y-auto scrollbar-hidden">
										<div className="grid grid-cols-1 lg:grid-cols-2 gap-3">
											{currentPatients.map((patient, index) => (
												<div
													key={patient.id}
													className="cursor-pointer"
													role="button"
													tabIndex={0}
													onClick={() => { handlePatientToggle(index); }}
													onKeyDown={(event_) => {
														if (event_.key === "Enter" || event_.key === " ") {
															handlePatientToggle(index);
														}
													}}
												>
													<PatientSelectionCard
														isSelected={selectedIndexes.includes(index)}
														patient={patient as unknown as SetupPatient}
													/>
												</div>
											))}
										</div>
									</div>
								</div>
							</div>
						</div>
					) : (
						renderStepContent()
					)}

					{/* Navigation Buttons */}
					<div className="flex items-center justify-between pt-6">
						{currentStep > 0 ? (
							<Button
								className="gap-2"
								variant="outline"
								onClick={handleBackStep}
							>
								<ChevronLeft className="w-4 h-4" />
								{t("back")}
							</Button>
						) : (
							<div></div>
						)}

						<Button
							className="gap-2"
							disabled={!canProceedToNextStep()}
							onClick={handleNextStep}
						>
							{currentStep === 3
								? isEditing
									? t("saveChanges")
									: t("completeSetup")
								: t("continue")}
							<ChevronRight className="w-4 h-4" />
						</Button>
					</div>
				</CardContent>
			</Card>
		</div>
	);
}
