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

	// Zustand selectors - only expandedSections needed now
	const expandedSections = useHandoverUIStore(state => state.expandedSections);
	const toggleSection = useHandoverUIStore(state => state.toggleSection);
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

	// CSS responsive layout: flex-col on mobile, 3-col grid on desktop
	// I-PASS sections with single tree pattern (Collapsible always mounted)
	return (
		<div className="flex flex-col gap-3 md:grid md:grid-cols-3 md:gap-6">
			{/* Left Column (desktop) - I, P, S */}
			<div className="md:col-span-2 space-y-3 md:space-y-6">
				{/* I - Illness Severity */}
				<HandoverSection
					description={sectionLabels.illness.description}
					guidelines={ipassGuidelines.illness}
					isExpanded={expandedSections.illness}
					isMobile={isMobile}
					letter="I"
					letterColor="blue"
					title={sectionLabels.illness.title}
					onToggle={() => { toggleSection('illness'); }}
				>
					<IllnessSeverity
						assignedPhysician={assignedPhysician}
						currentUser={currentUser}
						handoverId={handoverData.id}
						initialSeverity={patientData?.illnessSeverity ?? "stable"}
					/>
				</HandoverSection>

				{/* P - Patient Summary */}
				<HandoverSection
					description={sectionLabels.patient.description}
					guidelines={ipassGuidelines.patient}
					isExpanded={expandedSections.patient}
					isMobile={isMobile}
					letter="P"
					letterColor="blue"
					title={sectionLabels.patient.title}
					onToggle={() => { toggleSection('patient'); }}
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

				{/* S - Situation Awareness (on desktop, left column) */}
				<div className="hidden md:block">
					<HandoverSection
						description={sectionLabels.situation.description}
						guidelines={ipassGuidelines.awareness}
						isExpanded={expandedSections.awareness}
						isMobile={isMobile}
						letter="S"
						letterColor="blue"
						title={sectionLabels.situation.title}
						onToggle={() => { toggleSection('awareness'); }}
					>
						<SituationAwareness
							currentUser={currentUser}
							handoverId={handoverData.id}
							onRequestFullscreen={() => { handleOpenFullscreenEdit("situation-awareness", true); }}
						/>
					</HandoverSection>
				</div>
			</div>

			{/* Right Column (desktop) - A, S */}
			<div className="md:col-span-1 space-y-3 md:space-y-6">
				{/* A - Action List */}
				<HandoverSection
					description={sectionLabels.actions.description}
					guidelines={ipassGuidelines.actions}
					isExpanded={expandedSections.actions}
					isMobile={isMobile}
					letter="A"
					letterColor="blue"
					title={sectionLabels.actions.title}
					onToggle={() => { toggleSection('actions'); }}
				>
					<ActionList
						assignedPhysician={assignedPhysician}
						currentUser={currentUser}
						handoverId={handoverData?.id}
					/>
				</HandoverSection>

				{/* S - Situation Awareness (on mobile, after Action) */}
				<div className="md:hidden">
					<HandoverSection
						description={sectionLabels.situation.description}
						guidelines={ipassGuidelines.awareness}
						isExpanded={expandedSections.awareness}
						isMobile={isMobile}
						letter="S"
						letterColor="blue"
						title={sectionLabels.situation.title}
						onToggle={() => { toggleSection('awareness'); }}
					>
						<SituationAwareness
							currentUser={currentUser}
							handoverId={handoverData.id}
							onRequestFullscreen={() => { handleOpenFullscreenEdit("situation-awareness", true); }}
						/>
					</HandoverSection>
				</div>

				{/* S - Synthesis by Receiver */}
				<HandoverSection
					description={sectionLabels.synthesis.description}
					guidelines={ipassGuidelines.synthesis}
					isExpanded={expandedSections.synthesis}
					isMobile={isMobile}
					letter="S"
					letterColor="purple"
					title={sectionLabels.synthesis.title}
					onToggle={() => { toggleSection('synthesis'); }}
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
	);
}
