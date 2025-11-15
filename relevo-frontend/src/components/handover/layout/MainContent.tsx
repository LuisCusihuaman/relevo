import { getIpassGuidelines } from "@/common/constants";
import type {
	ExpandedSections,
	FullscreenComponent,
	SyncStatus,
} from "@/common/types";
import type { Handover, PatientHandoverData, User } from "@/api";
import {
	Collapsible,
	CollapsibleContent,
	CollapsibleTrigger,
} from "@/components/ui/collapsible";
import {
	Tooltip,
	TooltipContent,
	TooltipTrigger,
} from "@/components/ui/tooltip";
import { ChevronDown, ChevronUp, Info } from "lucide-react";
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
} from "..";

interface MainContentProps {
	layoutMode: "single" | "columns";
	expandedSections: ExpandedSections;
	getSessionDuration: () => string;
	handleOpenDiscussion: () => void;
	handleOpenFullscreenEdit: (
		component: FullscreenComponent,
		autoEdit?: boolean,
	) => void;
	handleToggleSection?: (section: keyof ExpandedSections) => void;
	syncStatus: SyncStatus;
	setSyncStatus: (status: SyncStatus) => void;
	setHandoverComplete: (complete: boolean) => void;
	currentUser: User | null;
	handoverData?: Handover;
    patientData: PatientHandoverData | null;
}

const toPhysician = (
	user: User | null,
): { id: string; name: string; initials: string; role: string } => {
	if (!user) {
		return {
			id: "unknown",
			name: "Unknown User",
			initials: "U",
			role: "Unknown",
		};
	}
	return {
		id: user.id,
		name: user.fullName ?? `${user.firstName} ${user.lastName}`,
		initials:
			(user.fullName ?? `${user.firstName} ${user.lastName}`)
				?.split(" ")
				.map((n) => n[0])
				.join("")
				.toUpperCase() ?? "",
		role: user.roles?.join(", ") ?? "",
	};
};

const formatPhysician = (
	physician: PatientHandoverData["assignedPhysician"] | PatientHandoverData["receivingPhysician"] | null | undefined,
): { name: string; initials: string; role: string } => {
	if (!physician) {
		return {
			name: "Unknown",
			initials: "U",
			role: "Doctor",
		};
	}
	return {
		name: physician.name,
		initials:
			physician.name
				?.split(" ")
				.map((n) => n[0])
				.join("")
				.toUpperCase() || "U",
		role: physician.role || "Doctor",
	};
};

export function MainContent({
	layoutMode,
	expandedSections,
	handleOpenDiscussion,
	handleOpenFullscreenEdit,
	handleToggleSection,
	syncStatus,
	setSyncStatus,
	setHandoverComplete,
	currentUser,
	handoverData,
    patientData,
}: MainContentProps): React.JSX.Element {
	const { t } = useTranslation(["handover", "mainContent"]);
	const ipassGuidelines = getIpassGuidelines(t);
	const handoverId = handoverData?.id;

	const {
		isLoading: isSituationAwarenessLoading,
		error: situationAwarenessError,
	} = useSituationAwareness(handoverId ?? "");
	const { isLoading: isSynthesisLoading, error: synthesisError } = useSynthesis(
		handoverId ?? "",
	);

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

	const assignedPhysician = {
		id: handoverData.responsiblePhysicianId,
		name: handoverData.responsiblePhysicianName,
		initials:
			(handoverData.responsiblePhysicianName || "")
				.split(" ")
				.map((n) => n[0])
				.join("")
				.toUpperCase() || "U",
		role: "Doctor",
	};

	return (
		<div className="space-y-8">
			{/* I-PASS Sections - Single Column Layout for Desktop */}
			{layoutMode === "columns" ? (
				<div className="hidden xl:block max-w-5xl mx-auto">
					<div className="space-y-8">
						{/* I - Illness Severity */}
						<Collapsible 
							open={expandedSections.illness}
							onOpenChange={() => handleToggleSection?.("illness")}
						>
							<div className="bg-white rounded-xl border-2 border-blue-100 shadow-sm">
								<CollapsibleTrigger asChild>
									<div className="p-6 border-b border-gray-200 bg-gradient-to-r from-blue-50 to-white cursor-pointer hover:bg-blue-50 transition-colors">
										<div className="flex items-center space-x-4">
											<div className="w-14 h-14 bg-blue-500 rounded-xl flex items-center justify-center shadow-md">
												<span className="font-bold text-white text-xl">I</span>
											</div>
											<div className="flex-1">
										<h3 className="font-semibold text-gray-900 text-lg">
											{t("mainContent:sections.illnessSeverity")}
										</h3>
										<p className="text-sm text-gray-600 mt-1">
											{t("mainContent:sections.illnessSeverityDescription")}
										</p>
											</div>
											<Tooltip>
												<TooltipTrigger asChild>
													<button className="w-5 h-5 flex items-center justify-center opacity-60 hover:opacity-100 transition-opacity">
														<Info className="w-4 h-4 text-gray-400" />
													</button>
												</TooltipTrigger>
												<TooltipContent
													className="bg-white border border-gray-200 shadow-lg p-4 max-w-sm"
													side="top"
												>
													<div className="space-y-2">
														<h4 className="font-medium text-gray-900 text-sm">
															{ipassGuidelines.illness.title}
														</h4>
														<ul className="space-y-1 text-xs text-gray-600">
															{ipassGuidelines.illness.points.map(
																(point, index) => (
																	<li
																		key={index}
																		className="flex items-start space-x-1"
																	>
																		<span className="text-gray-400 mt-0.5">•</span>
																		<span>{point}</span>
																	</li>
																)
															)}
														</ul>
													</div>
												</TooltipContent>
											</Tooltip>
										</div>
									</div>
								</CollapsibleTrigger>
								<CollapsibleContent>
									<div className="p-8">
										<IllnessSeverity
											assignedPhysician={assignedPhysician}
											currentUser={toPhysician(currentUser)}
										/>
									</div>
								</CollapsibleContent>
							</div>
						</Collapsible>

						{/* P - Patient Summary */}
						<Collapsible 
							open={expandedSections.patient}
							onOpenChange={() => handleToggleSection?.("patient")}
						>
							<div className="bg-white rounded-xl border-2 border-blue-100 shadow-sm">
								<CollapsibleTrigger asChild>
									<div className="p-6 border-b border-gray-200 bg-gradient-to-r from-blue-50 to-white cursor-pointer hover:bg-blue-50 transition-colors">
										<div className="flex items-center space-x-4">
											<div className="w-14 h-14 bg-blue-500 rounded-xl flex items-center justify-center shadow-md">
												<span className="font-bold text-white text-xl">P</span>
											</div>
											<div className="flex-1">
												<h3 className="font-semibold text-gray-900 text-lg">
													{t("mainContent:sections.patientSummary")}
												</h3>
												<p className="text-sm text-gray-600 mt-1">
													{t("mainContent:sections.patientSummaryDescription")}
												</p>
											</div>
											<Tooltip>
												<TooltipTrigger asChild>
													<button className="w-5 h-5 flex items-center justify-center opacity-60 hover:opacity-100 transition-opacity">
														<Info className="w-4 h-4 text-gray-400" />
													</button>
												</TooltipTrigger>
												<TooltipContent
													className="bg-white border border-gray-200 shadow-lg p-4 max-w-sm"
													side="top"
												>
													<div className="space-y-2">
														<h4 className="font-medium text-gray-900 text-sm">
															{ipassGuidelines.patient.title}
														</h4>
														<ul className="space-y-1 text-xs text-gray-600">
															{ipassGuidelines.patient.points.map(
																(point, index) => (
																	<li
																		key={index}
																		className="flex items-start space-x-1"
																	>
																		<span className="text-gray-400 mt-0.5">•</span>
																		<span>{point}</span>
																	</li>
																)
															)}
														</ul>
													</div>
												</TooltipContent>
											</Tooltip>
										</div>
									</div>
								</CollapsibleTrigger>
								<CollapsibleContent>
									<div className="p-8">
										<PatientSummary
											currentUser={toPhysician(currentUser)}
											handoverId={handoverData.id}
											handoverStateName={handoverData.stateName}
											patientData={patientData || undefined}
											responsiblePhysician={{
												id: handoverData.responsiblePhysicianId,
												name: handoverData.responsiblePhysicianName,
											}}
											syncStatus={syncStatus}
											onOpenThread={handleOpenDiscussion}
											onRequestFullscreen={() => {
												handleOpenFullscreenEdit("patient-summary");
											}}
											onSyncStatusChange={setSyncStatus}
										/>
									</div>
								</CollapsibleContent>
							</div>
						</Collapsible>

						{/* S - Current Situation */}
						<Collapsible 
							open={expandedSections.awareness}
							onOpenChange={() => handleToggleSection?.("awareness")}
						>
							<div className="bg-white rounded-xl border-2 border-blue-100 shadow-sm">
								<CollapsibleTrigger asChild>
									<div className="p-6 border-b border-gray-200 bg-gradient-to-r from-blue-50 to-white cursor-pointer hover:bg-blue-50 transition-colors">
										<div className="flex items-center space-x-4">
											<div className="w-14 h-14 bg-blue-500 rounded-xl flex items-center justify-center shadow-md">
												<span className="font-bold text-white text-xl">S</span>
											</div>
											<div className="flex-1">
												<h3 className="font-semibold text-gray-900 text-lg">
													{t("mainContent:sections.situationAwareness")}
												</h3>
												<p className="text-sm text-gray-600 mt-1">
													{t("mainContent:sections.situationAwarenessDescription")}
												</p>
											</div>
											<Tooltip>
												<TooltipTrigger asChild>
													<button className="w-5 h-5 flex items-center justify-center opacity-60 hover:opacity-100 transition-opacity">
														<Info className="w-4 h-4 text-gray-400" />
													</button>
												</TooltipTrigger>
												<TooltipContent
													className="bg-white border border-gray-200 shadow-lg p-4 max-w-sm"
													side="top"
												>
													<div className="space-y-2">
														<h4 className="font-medium text-gray-900 text-sm">
															{ipassGuidelines.awareness.title}
														</h4>
														<ul className="space-y-1 text-xs text-gray-600">
															{ipassGuidelines.awareness.points.map(
																(point, index) => (
																	<li
																		key={index}
																		className="flex items-start space-x-1"
																	>
																		<span className="text-gray-400 mt-0.5">•</span>
																		<span>{point}</span>
																	</li>
																)
															)}
														</ul>
													</div>
												</TooltipContent>
											</Tooltip>
										</div>
									</div>
								</CollapsibleTrigger>
								<CollapsibleContent>
									<div className="p-8">
										<SituationAwareness
											collaborators={[]}
											currentUser={toPhysician(currentUser)}
											handoverId={handoverData.id}
											syncStatus={syncStatus}
											onOpenThread={handleOpenDiscussion}
											onSyncStatusChange={setSyncStatus}
											onRequestFullscreen={() => {
												handleOpenFullscreenEdit("situation-awareness", true);
											}}
										/>
									</div>
								</CollapsibleContent>
							</div>
						</Collapsible>

						{/* A - Action List */}
						<Collapsible 
							open={expandedSections.actions}
							onOpenChange={() => handleToggleSection?.("actions")}
						>
							<div className="bg-white rounded-xl border-2 border-blue-100 shadow-sm">
								<CollapsibleTrigger asChild>
									<div className="p-6 border-b border-gray-200 bg-gradient-to-r from-blue-50 to-white cursor-pointer hover:bg-blue-50 transition-colors">
										<div className="flex items-center space-x-4">
											<div className="w-14 h-14 bg-blue-500 rounded-xl flex items-center justify-center shadow-md">
												<span className="font-bold text-white text-xl">A</span>
											</div>
											<div className="flex-1">
												<h3 className="font-semibold text-gray-900 text-lg">
													{t("mainContent:sections.actionList")}
												</h3>
												<p className="text-sm text-gray-600 mt-1">
													{t("mainContent:sections.actionListDescription")}
												</p>
											</div>
											<Tooltip>
												<TooltipTrigger asChild>
													<button className="w-5 h-5 flex items-center justify-center opacity-60 hover:opacity-100 transition-opacity">
														<Info className="w-4 h-4 text-gray-400" />
													</button>
												</TooltipTrigger>
												<TooltipContent
													className="bg-white border border-gray-200 shadow-lg p-4 max-w-sm"
													side="top"
												>
													<div className="space-y-2">
														<h4 className="font-medium text-gray-900 text-sm">
															{ipassGuidelines.actions.title}
														</h4>
														<ul className="space-y-1 text-xs text-gray-600">
															{ipassGuidelines.actions.points.map(
																(point, index) => (
																	<li
																		key={index}
																		className="flex items-start space-x-1"
																	>
																		<span className="text-gray-400 mt-0.5">•</span>
																		<span>{point}</span>
																	</li>
																)
															)}
														</ul>
													</div>
												</TooltipContent>
											</Tooltip>
										</div>
									</div>
								</CollapsibleTrigger>
								<CollapsibleContent>
									<div className="p-8">
										<ActionList
											expanded
											assignedPhysician={assignedPhysician}
											collaborators={[]}
											currentUser={toPhysician(currentUser)}
											handoverId={handoverData?.id}
											onOpenThread={handleOpenDiscussion}
										/>
									</div>
								</CollapsibleContent>
							</div>
						</Collapsible>

						{/* S - Synthesis by Receiver */}
						<Collapsible 
							open={expandedSections.synthesis}
							onOpenChange={() => handleToggleSection?.("synthesis")}
						>
							<div className="bg-white rounded-xl border-2 border-purple-100 shadow-sm">
								<CollapsibleTrigger asChild>
									<div className="p-6 border-b border-gray-200 bg-gradient-to-r from-purple-50 to-white cursor-pointer hover:bg-purple-50 transition-colors">
										<div className="flex items-center space-x-4">
											<div className="w-14 h-14 bg-purple-500 rounded-xl flex items-center justify-center shadow-md">
												<span className="font-bold text-white text-xl">S</span>
											</div>
											<div className="flex-1">
												<h3 className="font-semibold text-gray-900 text-lg">
													{t("mainContent:sections.synthesisByReceiver")}
												</h3>
												<p className="text-sm text-gray-600 mt-1">
													{t(
														"mainContent:sections.synthesisByReceiverDescription",
													)}
												</p>
											</div>
											<Tooltip>
												<TooltipTrigger asChild>
													<button className="w-5 h-5 flex items-center justify-center opacity-60 hover:opacity-100 transition-opacity">
														<Info className="w-4 h-4 text-gray-400" />
													</button>
												</TooltipTrigger>
												<TooltipContent
													className="bg-white border border-gray-200 shadow-lg p-4 max-w-sm"
													side="top"
												>
													<div className="space-y-2">
														<h4 className="font-medium text-gray-900 text-sm">
															{ipassGuidelines.synthesis.title}
														</h4>
														<ul className="space-y-1 text-xs text-gray-600">
															{ipassGuidelines.synthesis.points.map(
																(point, index) => (
																	<li
																		key={index}
																		className="flex items-start space-x-1"
																	>
																		<span className="text-gray-400 mt-0.5">•</span>
																		<span>{point}</span>
																	</li>
																)
															)}
														</ul>
													</div>
												</TooltipContent>
											</Tooltip>
										</div>
									</div>
								</CollapsibleTrigger>
								<CollapsibleContent>
									<div className="p-8">
										<SynthesisByReceiver
											currentUser={toPhysician(currentUser)}
											handoverComplete={handoverData.status === "Completed"}
											handoverState={handoverData.status}
											receivingPhysician={formatPhysician(
												patientData?.receivingPhysician,
											)}
											onComplete={setHandoverComplete}
											onOpenThread={handleOpenDiscussion}
										/>
									</div>
								</CollapsibleContent>
							</div>
						</Collapsible>
					</div>
				</div>
			) : null}

			{/* Single Column Layout - Subtle Borders & I-PASS Guidelines */}
			<div
				className={`space-y-6 ${layoutMode === "columns" ? "xl:hidden" : ""}`}
			>
				{/* I - Illness Severity */}
				<Collapsible asChild>
					<div className="bg-white rounded-xl border-2 border-blue-100 shadow-sm overflow-hidden">
						<CollapsibleTrigger asChild>
							<div className="p-6 bg-gradient-to-r from-blue-50 to-white border-b border-gray-200 cursor-pointer hover:bg-blue-50 transition-colors">
								<div className="flex items-center justify-between">
									<div className="flex items-center space-x-4">
										<div className="w-14 h-14 bg-blue-500 rounded-xl flex items-center justify-center shadow-md">
											<span className="font-bold text-white text-xl">I</span>
										</div>
										<div>
											<div className="flex items-center space-x-2">
												<h3 className="font-semibold text-gray-900 text-lg">
													{t("mainContent:sections.illnessSeverity")}
												</h3>
												<Tooltip>
													<TooltipTrigger asChild>
														<button className="w-4 h-4 flex items-center justify-center opacity-60 hover:opacity-100 transition-opacity">
															<Info className="w-3 h-3 text-gray-400" />
														</button>
													</TooltipTrigger>
													<TooltipContent
														className="bg-white border border-gray-200 shadow-lg p-4 max-w-sm"
														side="top"
													>
														<div className="space-y-2">
															<h4 className="font-medium text-gray-900 text-sm">
																{ipassGuidelines.illness.title}
															</h4>
															<ul className="space-y-1 text-xs text-gray-600">
																{ipassGuidelines.illness.points.map(
																	(point, index) => (
																		<li
																			key={index}
																			className="flex items-start space-x-1"
																		>
																			<span className="text-gray-400 mt-0.5">•</span>
																			<span>{point}</span>
																		</li>
																	)
																)}
															</ul>
														</div>
													</TooltipContent>
												</Tooltip>
											</div>
											<p className="text-sm text-gray-700">
												{t("mainContent:sections.illnessSeverityDescription")}
											</p>
										</div>
										<div className="flex items-center">
											{expandedSections.illness ? (
												<ChevronUp className="w-4 h-4 text-gray-500" />
											) : (
												<ChevronDown className="w-4 h-4 text-gray-500" />
											)}
										</div>
									</div>
								</div>
							</div>
						</CollapsibleTrigger>
						<CollapsibleContent>
							<div className="p-8">
								<IllnessSeverity
									assignedPhysician={assignedPhysician}
									currentUser={toPhysician(currentUser)}
								/>
									</div>
								</CollapsibleContent>
							</div>
						</Collapsible>

				{/* P - Patient Summary */}
				<Collapsible asChild>
					<div className="bg-white rounded-xl border-2 border-blue-100 shadow-sm overflow-hidden">
						<CollapsibleTrigger asChild>
							<div className="p-6 bg-gradient-to-r from-blue-50 to-white border-b border-gray-200 cursor-pointer hover:bg-blue-50 transition-colors">
								<div className="flex items-center justify-between">
									<div className="flex items-center space-x-4">
										<div className="w-14 h-14 bg-blue-500 rounded-xl flex items-center justify-center shadow-md">
											<span className="font-bold text-white text-xl">P</span>
										</div>
										<div>
											<div className="flex items-center space-x-2">
												<h3 className="font-semibold text-gray-900 text-lg">
													{t("mainContent:sections.patientSummary")}
												</h3>
												<Tooltip>
													<TooltipTrigger asChild>
														<button className="w-4 h-4 flex items-center justify-center opacity-60 hover:opacity-100 transition-opacity">
															<Info className="w-3 h-3 text-gray-400" />
														</button>
													</TooltipTrigger>
													<TooltipContent
														className="bg-white border border-gray-200 shadow-lg p-4 max-w-sm"
														side="top"
													>
														<div className="space-y-2">
															<h4 className="font-medium text-gray-900 text-sm">
																{ipassGuidelines.patient.title}
															</h4>
															<ul className="space-y-1 text-xs text-gray-600">
																{ipassGuidelines.patient.points.map(
																	(point, index) => (
																		<li
																			key={index}
																			className="flex items-start space-x-1"
																		>
																			<span className="text-gray-400 mt-0.5">•</span>
																			<span>{point}</span>
																		</li>
																	)
																)}
															</ul>
														</div>
													</TooltipContent>
												</Tooltip>
											</div>
											<p className="text-sm text-gray-700">
												{t("mainContent:sections.patientSummaryDescription")}
											</p>
										</div>
										<div className="flex items-center">
											{expandedSections.patient ? (
												<ChevronUp className="w-4 h-4 text-gray-500" />
											) : (
												<ChevronDown className="w-4 h-4 text-gray-500" />
											)}
										</div>
									</div>
								</div>
							</div>
						</CollapsibleTrigger>
						<CollapsibleContent>
							<div className="p-8">
								<PatientSummary
									currentUser={toPhysician(currentUser)}
									handoverId={handoverData.id}
									handoverStateName={handoverData.stateName}
									patientData={patientData || undefined}
									responsiblePhysician={{
										id: handoverData.responsiblePhysicianId,
										name: handoverData.responsiblePhysicianName,
									}}
									syncStatus={syncStatus}
									onOpenThread={handleOpenDiscussion}
									onRequestFullscreen={() => {
										handleOpenFullscreenEdit("patient-summary");
									}}
									onSyncStatusChange={setSyncStatus}
								/>
									</div>
								</CollapsibleContent>
							</div>
						</Collapsible>

				{/* A - Action List */}
				<Collapsible asChild>
					<div className="bg-white rounded-xl border-2 border-blue-100 shadow-sm overflow-hidden">
						<CollapsibleTrigger asChild>
							<div className="p-6 bg-gradient-to-r from-blue-50 to-white border-b border-gray-200 cursor-pointer hover:bg-blue-50 transition-colors">
								<div className="flex items-center justify-between">
									<div className="flex items-center space-x-4">
										<div className="w-14 h-14 bg-blue-500 rounded-xl flex items-center justify-center shadow-md">
											<span className="font-bold text-white text-xl">A</span>
										</div>
										<div>
											<div className="flex items-center space-x-2">
												<h3 className="font-semibold text-gray-900 text-lg">
													{t("mainContent:sections.actionList")}
												</h3>
												<Tooltip>
													<TooltipTrigger asChild>
														<button className="w-4 h-4 flex items-center justify-center opacity-60 hover:opacity-100 transition-opacity">
															<Info className="w-3 h-3 text-gray-400" />
														</button>
													</TooltipTrigger>
													<TooltipContent
														className="bg-white border border-gray-200 shadow-lg p-4 max-w-sm"
														side="top"
													>
														<div className="space-y-2">
															<h4 className="font-medium text-gray-900 text-sm">
																{ipassGuidelines.actions.title}
															</h4>
															<ul className="space-y-1 text-xs text-gray-600">
																{ipassGuidelines.actions.points.map(
																	(point, index) => (
																		<li
																			key={index}
																			className="flex items-start space-x-1"
																		>
																			<span className="text-gray-400 mt-0.5">•</span>
																			<span>{point}</span>
																		</li>
																	)
																)}
															</ul>
														</div>
													</TooltipContent>
												</Tooltip>
											</div>
											<p className="text-sm text-gray-700">
												{t("mainContent:sections.actionListDescription")}
											</p>
										</div>
										<div className="flex items-center">
											{expandedSections.actions ? (
												<ChevronUp className="w-4 h-4 text-gray-500" />
											) : (
												<ChevronDown className="w-4 h-4 text-gray-500" />
											)}
										</div>
									</div>
								</div>
							</div>
						</CollapsibleTrigger>
						<CollapsibleContent>
							<div className="p-8">
								<ActionList
									expanded
									assignedPhysician={assignedPhysician}
									collaborators={[]}
									currentUser={toPhysician(currentUser)}
									handoverId={handoverData?.id}
									onOpenThread={handleOpenDiscussion}
								/>
									</div>
								</CollapsibleContent>
							</div>
						</Collapsible>

				{/* S - Current Situation */}
				<Collapsible asChild>
					<div className="bg-white rounded-xl border-2 border-blue-100 shadow-sm overflow-hidden">
						<CollapsibleTrigger asChild>
							<div className="p-6 bg-gradient-to-r from-blue-50 to-white border-b border-gray-200 cursor-pointer hover:bg-blue-50 transition-colors">
								<div className="flex items-center justify-between">
									<div className="flex items-center space-x-4">
										<div className="w-14 h-14 bg-blue-500 rounded-xl flex items-center justify-center shadow-md">
											<span className="font-bold text-white text-xl">S</span>
										</div>
										<div>
											<div className="flex items-center space-x-2">
												<h3 className="font-semibold text-gray-900 text-lg">
													{t("mainContent:sections.situationAwareness")}
												</h3>
												<Tooltip>
													<TooltipTrigger asChild>
														<button className="w-4 h-4 flex items-center justify-center opacity-60 hover:opacity-100 transition-opacity">
															<Info className="w-3 h-3 text-gray-400" />
														</button>
													</TooltipTrigger>
													<TooltipContent
														className="bg-white border border-gray-200 shadow-lg p-4 max-w-sm"
														side="top"
													>
														<div className="space-y-2">
															<h4 className="font-medium text-gray-900 text-sm">
																{ipassGuidelines.awareness.title}
															</h4>
															<ul className="space-y-1 text-xs text-gray-600">
																{ipassGuidelines.awareness.points.map(
																	(point, index) => (
																		<li
																			key={index}
																			className="flex items-start space-x-1"
																		>
																			<span className="text-gray-400 mt-0.5">•</span>
																			<span>{point}</span>
																		</li>
																	)
																)}
															</ul>
														</div>
													</TooltipContent>
												</Tooltip>
											</div>
											<p className="text-sm text-gray-700">
												{t(
													"mainContent:sections.situationAwarenessDescription",
												)}
											</p>
										</div>
										<div className="flex items-center">
											{expandedSections.awareness ? (
												<ChevronUp className="w-4 h-4 text-gray-500" />
											) : (
												<ChevronDown className="w-4 h-4 text-gray-500" />
											)}
										</div>
									</div>
								</div>
							</div>
						</CollapsibleTrigger>
						<CollapsibleContent>
							<div className="p-8">
								<SituationAwareness
									collaborators={[]}
									currentUser={toPhysician(currentUser)}
									handoverId={handoverData.id}
									syncStatus={syncStatus}
									onOpenThread={handleOpenDiscussion}
									onSyncStatusChange={setSyncStatus}
									onRequestFullscreen={() => {
										handleOpenFullscreenEdit("situation-awareness", true);
									}}
								/>
									</div>
								</CollapsibleContent>
							</div>
						</Collapsible>

				{/* S - Synthesis by Receiver */}
				<Collapsible asChild>
					<div className="bg-white rounded-xl border-2 border-purple-100 shadow-sm overflow-hidden">
						<CollapsibleTrigger asChild>
							<div className="p-6 bg-gradient-to-r from-purple-50 to-white border-b border-gray-200 cursor-pointer hover:bg-purple-50 transition-colors">
								<div className="flex items-center justify-between">
									<div className="flex items-center space-x-4">
										<div className="w-14 h-14 bg-purple-500 rounded-xl flex items-center justify-center shadow-md">
											<span className="font-bold text-white text-xl">S</span>
										</div>
										<div>
											<div className="flex items-center space-x-2">
												<h3 className="font-semibold text-gray-900 text-lg">
													{t("mainContent:sections.synthesisByReceiver")}
												</h3>
												<Tooltip>
													<TooltipTrigger asChild>
														<button className="w-4 h-4 flex items-center justify-center opacity-60 hover:opacity-100 transition-opacity">
															<Info className="w-3 h-3 text-gray-400" />
														</button>
													</TooltipTrigger>
													<TooltipContent
														className="bg-white border border-gray-200 shadow-lg p-4 max-w-sm"
														side="top"
													>
														<div className="space-y-2">
															<h4 className="font-medium text-gray-900 text-sm">
																{ipassGuidelines.synthesis.title}
															</h4>
															<ul className="space-y-1 text-xs text-gray-600">
																{ipassGuidelines.synthesis.points.map(
																	(point, index) => (
																		<li
																			key={index}
																			className="flex items-start space-x-1"
																		>
																			<span className="text-gray-400 mt-0.5">•</span>
																			<span>{point}</span>
																		</li>
																	)
																)}
															</ul>
														</div>
													</TooltipContent>
												</Tooltip>
											</div>
											<p className="text-sm text-gray-700">
												{t(
													"mainContent:sections.synthesisByReceiverDescription",
												)}
											</p>
										</div>
										<div className="flex items-center">
											{expandedSections.synthesis ? (
												<ChevronUp className="w-4 h-4 text-gray-500" />
											) : (
												<ChevronDown className="w-4 h-4 text-gray-500" />
											)}
										</div>
									</div>
								</div>
							</div>
						</CollapsibleTrigger>
						<CollapsibleContent>
							<div className="p-8">
								<SynthesisByReceiver
									currentUser={toPhysician(currentUser)}
									handoverComplete={handoverData.status === "Completed"}
									handoverState={handoverData.status}
									receivingPhysician={formatPhysician(
										patientData?.receivingPhysician,
									)}
									onComplete={setHandoverComplete}
									onOpenThread={handleOpenDiscussion}
								/>
									</div>
								</CollapsibleContent>
							</div>
						</Collapsible>
			</div>
		</div>
	);
}
