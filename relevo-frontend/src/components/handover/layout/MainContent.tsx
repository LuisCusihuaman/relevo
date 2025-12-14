import { getIpassGuidelines } from "@/common/constants";
import type { FullscreenComponent } from "@/types/domain";

import { useTranslation } from "react-i18next";
import { toast } from "sonner";
import {
	useSituationAwareness,
	useSynthesis,
	useCompleteHandover,
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
import { useIsMobile } from "@/hooks/use-mobile";

export function MainContent(): React.JSX.Element {
	const { t } = useTranslation(["handover", "mainContent"]);
	const isMobile = useIsMobile();

	// Use Context
	const {
		handoverId,
		handoverData,
		patientData,
		currentUser,
		assignedPhysician,
		receivingPhysician,
	} = useCurrentHandover();

	// Zustand selectors
	const expandedSections = useHandoverUIStore(state => state.expandedSections);
	const setExpandedSection = useHandoverUIStore(state => state.setExpandedSection);
	const setFullscreenEditing = useHandoverUIStore(state => state.setFullscreenEditing);

	const ipassGuidelines = getIpassGuidelines(t);

	const { mutate: completeHandover } = useCompleteHandover();

	const handleConfirmHandover = (): void => {
		if (!handoverData?.id) return;
		completeHandover(handoverData.id, {
			onSuccess: () => { toast.success("Handover completed successfully"); },
			onError: (err) => { toast.error(`Failed to complete handover: ${err.message}`); },
		});
	};

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

	const responsiblePhysician = {
		id: handoverData.responsiblePhysicianId,
		name: handoverData.responsiblePhysicianName,
	};

	// Section labels
	const sectionLabels = {
		illness: {
			title: t("mainContent:sections.illnessSeverity"),
			description: t("mainContent:sections.illnessSeverityDescription"),
		},
		patient: {
			title: t("mainContent:sections.patientSummary"),
			description: t("mainContent:sections.patientSummaryDescription"),
		},
		situation: {
			title: t("mainContent:sections.situationAwareness"),
			description: t("mainContent:sections.situationAwarenessDescription"),
		},
		actions: {
			title: t("mainContent:sections.actionList"),
			description: t("mainContent:sections.actionListDescription"),
		},
		synthesis: {
			title: t("mainContent:sections.synthesisByReceiver"),
			description: t("mainContent:sections.synthesisByReceiverDescription"),
		},
	};

	/*
	 * Layout Strategy:
	 * - Mobile (flex-col): I → P → A → S(Situation) → S(Synthesis) using CSS order
	 *   Column wrappers use `contents` to flatten, allowing order to work across all items
	 * - Desktop (flex-row with two independent columns):
	 *     Left (2/3): I, P, S-Situation flow vertically
	 *     Right (1/3): A, S-Synthesis flow vertically
	 *   Each column flows independently - no row alignment issues
	 * 
	 * Single instance of each component (no unmount/mount on resize)
	 */
	return (
		<div className="flex flex-col gap-3 md:flex-row md:gap-6">
			{/* Left Column - contents on mobile to flatten for order, flex-col on desktop */}
			<div className="contents md:flex md:flex-col md:w-2/3 md:gap-6">
				{/* I - Illness Severity */}
				<div className="order-1 md:order-none">
					<HandoverSection
						description={sectionLabels.illness.description}
						guidelines={ipassGuidelines.illness}
						isExpanded={expandedSections.illness}
						isMobile={isMobile}
						letter="I"
						letterColor="blue"
						title={sectionLabels.illness.title}
						onOpenChange={(open) => { setExpandedSection('illness', open); }}
					>
						<IllnessSeverity
							assignedPhysician={assignedPhysician}
							currentUser={currentUser}
							handoverId={handoverData.id}
							initialSeverity={patientData?.illnessSeverity ?? "stable"}
						/>
					</HandoverSection>
				</div>

				{/* P - Patient Summary */}
				<div className="order-2 md:order-none">
					<HandoverSection
						description={sectionLabels.patient.description}
						guidelines={ipassGuidelines.patient}
						isExpanded={expandedSections.patient}
						isMobile={isMobile}
						letter="P"
						letterColor="blue"
						title={sectionLabels.patient.title}
						onOpenChange={(open) => { setExpandedSection('patient', open); }}
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
				</div>

				{/* S - Situation Awareness */}
				<div className="order-4 md:order-none">
					<HandoverSection
						description={sectionLabels.situation.description}
						guidelines={ipassGuidelines.awareness}
						isExpanded={expandedSections.awareness}
						isMobile={isMobile}
						letter="S"
						letterColor="blue"
						title={sectionLabels.situation.title}
						onOpenChange={(open) => { setExpandedSection('awareness', open); }}
					>
						<SituationAwareness
							currentUser={currentUser}
							handoverId={handoverData.id}
							onRequestFullscreen={() => { handleOpenFullscreenEdit("situation-awareness", true); }}
						/>
					</HandoverSection>
				</div>
			</div>

			{/* Right Column - contents on mobile to flatten for order, flex-col on desktop */}
			<div className="contents md:flex md:flex-col md:w-1/3 md:gap-6">
				{/* A - Action List */}
				<div className="order-3 md:order-none">
					<HandoverSection
						description={sectionLabels.actions.description}
						guidelines={ipassGuidelines.actions}
						isExpanded={expandedSections.actions}
						isMobile={isMobile}
						letter="A"
						letterColor="blue"
						title={sectionLabels.actions.title}
						onOpenChange={(open) => { setExpandedSection('actions', open); }}
					>
						<ActionList
							assignedPhysician={assignedPhysician}
							currentUser={currentUser}
							handoverId={handoverData?.id}
						/>
					</HandoverSection>
				</div>

				{/* S - Synthesis by Receiver */}
				<div className="order-5 md:order-none">
					<HandoverSection
						description={sectionLabels.synthesis.description}
						guidelines={ipassGuidelines.synthesis}
						isExpanded={expandedSections.synthesis}
						isMobile={isMobile}
						letter="S"
						letterColor="purple"
						title={sectionLabels.synthesis.title}
						onOpenChange={(open) => { setExpandedSection('synthesis', open); }}
					>
						<SynthesisByReceiver
							currentUser={currentUser}
							handoverComplete={handoverData.stateName === "Completed"}
							handoverState={handoverData.stateName}
							receivingPhysician={receivingPhysician}
							onConfirm={handleConfirmHandover}
						/>
					</HandoverSection>
				</div>
			</div>
		</div>
	);
}
