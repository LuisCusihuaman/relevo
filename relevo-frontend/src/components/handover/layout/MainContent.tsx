import { getIpassGuidelines } from "@/common/constants";
import type {
	FullscreenComponent,
} from "@/common/types";
import { useTranslation } from "react-i18next";
import {
	useSituationAwareness,
	useSynthesis,
} from "@/api/endpoints/handovers";
import {
	ActionList,
	IllnessSeverity,
	PatientSummary,
	SituationAwareness,
	SynthesisByReceiver,
    useCurrentHandover
} from "..";
import { HandoverSection } from "./HandoverSection";
import { useHandoverUIStore } from "@/store/handover-ui.store";

export function MainContent(): React.JSX.Element {
	const { t } = useTranslation(["handover", "mainContent"]);
    
    // Use Context
    const { 
        handoverId, 
        handoverData, 
        patientData, 
        currentUser, 
        assignedPhysician, 
        receivingPhysician,
    } = useCurrentHandover();
    
    // Optimized Zustand selectors
    const layoutMode = useHandoverUIStore(state => state.layoutMode);
    const expandedSections = useHandoverUIStore(state => state.expandedSections);
    const setFullscreenEditing = useHandoverUIStore(state => state.setFullscreenEditing);

	const ipassGuidelines = getIpassGuidelines(t);

	const {
		isLoading: isSituationAwarenessLoading,
		error: situationAwarenessError,
	} = useSituationAwareness(handoverId ?? "");
	const { isLoading: isSynthesisLoading, error: synthesisError } = useSynthesis(
		handoverId ?? "",
	);

    const handleOpenFullscreenEdit = (component: FullscreenComponent, autoEdit: boolean = true): void => {
        setFullscreenEditing({ component, autoEdit });
    };

	// Loading state
	if (isSituationAwarenessLoading || isSynthesisLoading) {
		return (
			<div className="flex items-center justify-center p-8">
				<div className="text-center">
					<div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600 mx-auto mb-4" />
					<p className="text-gray-600">
						{t("mainContent:loadingPatientData")}
					</p>
				</div>
			</div>
		);
	}

	// Error state
	if (situationAwarenessError || synthesisError) {
		return (
			<div className="flex items-center justify-center p-8">
				<div className="text-center">
					<div className="text-red-500 mb-4">⚠️</div>
					<p className="text-red-600">
						{t("mainContent:errorLoadingPatientData")}
					</p>
					<p className="text-sm text-gray-500 mt-2">
						{situationAwarenessError?.message ||
							synthesisError?.message}
					</p>
				</div>
			</div>
		);
	}

	// No data state
	if (!handoverData) {
		return (
			<div className="flex items-center justify-center p-8">
				<div className="text-center">
					<div className="text-gray-500 mb-4">📋</div>
					<p className="text-gray-600">
						{t("mainContent:noPatientData")}
					</p>
				</div>
			</div>
		);
	}

	const isCollapsible = layoutMode === "single";

	const responsiblePhysician = {
		id: handoverData.responsiblePhysicianId,
		name: handoverData.responsiblePhysicianName,
	};
    
	// Section labels (casted to string to satisfy strict typing)
	const sectionLabels = {
		illness: {
			title: t("mainContent:sections.illnessSeverity") as string,
			description: t("mainContent:sections.illnessSeverityDescription") as string,
		},
		patient: {
			title: t("mainContent:sections.patientSummary") as string,
			description: t("mainContent:sections.patientSummaryDescription") as string,
		},
		situation: {
			title: t("mainContent:sections.situationAwareness") as string,
			description: t("mainContent:sections.situationAwarenessDescription") as string,
		},
		actions: {
			title: t("mainContent:sections.actionList") as string,
			description: t("mainContent:sections.actionListDescription") as string,
		},
		synthesis: {
			title: t("mainContent:sections.synthesisByReceiver") as string,
			description: t("mainContent:sections.synthesisByReceiverDescription") as string,
		},
	};

	// I-PASS Sections rendered ONCE, layout controlled by CSS
	const IllnessSection = (
		<HandoverSection
			collapsible={isCollapsible}
			description={sectionLabels.illness.description}
			guidelines={ipassGuidelines.illness}
			isExpanded={expandedSections.illness}
			letter="I"
			letterColor="blue"
			title={sectionLabels.illness.title}
		>
			<IllnessSeverity
				assignedPhysician={assignedPhysician}
				currentUser={currentUser}
			/>
		</HandoverSection>
	);

	const PatientSection = (
		<HandoverSection
			collapsible={isCollapsible}
			description={sectionLabels.patient.description}
			guidelines={ipassGuidelines.patient}
			isExpanded={expandedSections.patient}
			letter="P"
			letterColor="blue"
			title={sectionLabels.patient.title}
		>
			<PatientSummary
				currentUser={currentUser}
				handoverId={handoverData.id}
				handoverStateName={handoverData.stateName}
				patientData={patientData || undefined}
				responsiblePhysician={responsiblePhysician}
				onRequestFullscreen={() => { handleOpenFullscreenEdit("patient-summary"); }}
			/>
		</HandoverSection>
	);

	const SituationSection = (
		<HandoverSection
			collapsible={isCollapsible}
			description={sectionLabels.situation.description}
			guidelines={ipassGuidelines.awareness}
			isExpanded={expandedSections.awareness}
			letter="S"
			letterColor="blue"
			title={sectionLabels.situation.title}
		>
			<SituationAwareness
				currentUser={currentUser}
				handoverId={handoverData.id}
				onRequestFullscreen={() => { handleOpenFullscreenEdit("situation-awareness", true); }}
			/>
		</HandoverSection>
	);

	const ActionSection = (
		<HandoverSection
			collapsible={isCollapsible}
			description={sectionLabels.actions.description}
			guidelines={ipassGuidelines.actions}
			isExpanded={expandedSections.actions}
			letter="A"
			letterColor="blue"
			title={sectionLabels.actions.title}
		>
			<ActionList
				assignedPhysician={assignedPhysician}
				currentUser={currentUser}
				handoverId={handoverData?.id}
			/>
		</HandoverSection>
	);

	const SynthesisSection = (
		<HandoverSection
			collapsible={isCollapsible}
			description={sectionLabels.synthesis.description}
			guidelines={ipassGuidelines.synthesis}
			isExpanded={expandedSections.synthesis}
			letter="S"
			letterColor="purple"
			title={sectionLabels.synthesis.title}
		>
			<SynthesisByReceiver
				currentUser={currentUser}
				handoverComplete={handoverData.stateName === "Completed"}
				handoverState={handoverData.stateName}
				receivingPhysician={receivingPhysician}
			/>
		</HandoverSection>
	);

	// Single column (mobile/collapsible) layout
	if (layoutMode === "single") {
		return (
			<div className="space-y-3">
				{IllnessSection}
				{PatientSection}
				{ActionSection}
				{SituationSection}
				{SynthesisSection}
			</div>
		);
	}

	// Desktop: 3-column grid layout
	return (
		<div className="grid xl:grid-cols-3 xl:gap-8 gap-6">
			{/* Left Column - Main Sections */}
			<div className="xl:col-span-2 space-y-6">
				{IllnessSection}
				{PatientSection}
				{SituationSection}
			</div>

			{/* Right Column - Actions & Synthesis */}
			<div className="xl:col-span-1 space-y-6">
				{ActionSection}
				{SynthesisSection}
			</div>
		</div>
	);
}
