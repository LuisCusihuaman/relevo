import { getIpassGuidelines, activeCollaborators } from "@/common/constants";
import type {
  ExpandedSections,
  FullscreenComponent,
  SyncStatus,
  User,
} from "@/common/types";
import type { Handover } from "@/api";
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
import { usePatientHandoverData } from "@/hooks/usePatientHandoverData";
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
  handleOpenDiscussion: () => void;
  handleOpenFullscreenEdit: (
    component: FullscreenComponent,
    autoEdit?: boolean,
  ) => void;
  syncStatus: SyncStatus;
  setSyncStatus: (status: SyncStatus) => void;
  setHandoverComplete: (complete: boolean) => void;
  getSessionDuration: () => string;
  currentUser: User;
  handoverData?: Handover;
}

export function MainContent({
  layoutMode,
  expandedSections,
  handleOpenDiscussion,
  handleOpenFullscreenEdit,
  syncStatus,
  setSyncStatus,
  setHandoverComplete,
  getSessionDuration,
  currentUser,
  handoverData,
}: MainContentProps): React.JSX.Element {
  const { t } = useTranslation(["handover", "mainContent"]);
  const ipassGuidelines = getIpassGuidelines(t);

  // Use the new hook for patient handover data
  const { patientData: currentPatientData, isLoading: isPatientLoading, error: patientError } = usePatientHandoverData(handoverData);

  // Get active users from handover data or fallback to empty array
  const activeUsers: any[] = []; // TODO: Implement when participants are available

  // Handle loading state
  if (isPatientLoading) {
    return (
      <div className="flex items-center justify-center p-8">
        <div className="text-center">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600 mx-auto mb-4"></div>
          <p className="text-gray-600">{t("mainContent:loadingPatientData")}</p>
        </div>
      </div>
    );
  }

  // Handle error state
  if (patientError) {
    return (
      <div className="flex items-center justify-center p-8">
        <div className="text-center">
          <div className="text-red-500 mb-4">⚠️</div>
          <p className="text-red-600">{t("mainContent:errorLoadingPatientData")}</p>
          <p className="text-sm text-gray-500 mt-2">{patientError.message}</p>
        </div>
      </div>
    );
  }

  // Handle case where no patient data is available
  if (!currentPatientData) {
    return (
      <div className="flex items-center justify-center p-8">
        <div className="text-center">
          <div className="text-gray-500 mb-4">📋</div>
          <p className="text-gray-600">{t("mainContent:noPatientData")}</p>
        </div>
      </div>
    );
  }

  // Helper function to get section content from handover data
  const getSectionContent = (sectionType: string): string | null => {
    if (!handoverData) return null;

    // Map section types to handover properties
    switch (sectionType) {
      case "patient_summary":
        return handoverData.patientSummary?.content || null;
      case "situation_awareness":
        return handoverData.situationAwarenessDocId || null;
      case "synthesis":
        return handoverData.synthesis?.content || null;
      default:
        return null;
    }
  };

  // Helper function to get section status
  const getSectionStatus = (sectionType: string): string => {
    if (!handoverData) return "draft";
    // For now, return "draft" as default since we don't have section status in basic Handover type
    return "draft";
  };


  // Normal Mode
  return (
    <div className="space-y-6">
      {/* I-PASS Sections - Column Layout for Desktop */}
      {layoutMode === "columns" ? (
        <div className="hidden xl:grid xl:grid-cols-3 xl:gap-8">
          {/* Left Column */}
          <div className="xl:col-span-2 space-y-6">
            {/* I - Illness Severity */}
            <div className="bg-white rounded-lg border border-gray-100">
              <div className="p-4 border-b border-gray-100">
                <div className="flex items-center space-x-3">
                  <div className="w-8 h-8 bg-blue-100 rounded-full flex items-center justify-center">
                    <span className="font-bold text-blue-700">I</span>
                  </div>
                  <div className="flex-1">
                    <h3 className="font-medium text-gray-900">
                      {t("mainContent:sections.illnessSeverity")}
                    </h3>
                    <p className="text-sm text-gray-600">
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
                            ),
                          )}
                        </ul>
                      </div>
                    </TooltipContent>
                  </Tooltip>
                </div>
              </div>
              <div className="p-6">
                <IllnessSeverity
                  assignedPhysician={currentPatientData.assignedPhysician}
                  currentUser={currentUser}
                />
              </div>
            </div>

            {/* P - Patient Summary */}
            <div className="bg-white rounded-lg border border-gray-100">
              <div className="p-4 border-b border-gray-100">
                <div className="flex items-center space-x-3">
                  <div className="w-8 h-8 bg-blue-100 rounded-full flex items-center justify-center">
                    <span className="font-bold text-blue-700">P</span>
                  </div>
                  <div className="flex-1">
                    <h3 className="font-medium text-gray-900">
                      {t("mainContent:sections.patientSummary")}
                    </h3>
                    <p className="text-sm text-gray-600">
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
                            ),
                          )}
                        </ul>
                      </div>
                    </TooltipContent>
                  </Tooltip>
                </div>
              </div>
              <PatientSummary
                patientId={currentPatientData.id}
                assignedPhysician={currentPatientData.assignedPhysician}
                currentUser={currentUser}
                syncStatus={syncStatus}
                onOpenThread={handleOpenDiscussion}
                onSyncStatusChange={setSyncStatus}
                onRequestFullscreen={() =>
                  { handleOpenFullscreenEdit("patient-summary"); }
                }
              />
            </div>

            {/* S - Current Situation */}
            <div className="bg-white rounded-lg border border-gray-100">
              <div className="p-4 border-b border-gray-100">
                <div className="flex items-center space-x-3">
                  <div className="w-8 h-8 bg-blue-100 rounded-full flex items-center justify-center">
                    <span className="font-bold text-blue-700">S</span>
                  </div>
                  <div className="flex-1">
                    <h3 className="font-medium text-gray-900">
                      {t("mainContent:sections.situationAwareness")}
                    </h3>
                    <p className="text-sm text-gray-600">
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
                            ),
                          )}
                        </ul>
                      </div>
                    </TooltipContent>
                  </Tooltip>
                </div>
              </div>
              <SituationAwareness
                handoverId={handoverData.id}
                collaborators={activeCollaborators}
                syncStatus={syncStatus}
                onOpenThread={handleOpenDiscussion}
                onSyncStatusChange={setSyncStatus}
                onRequestFullscreen={() =>
                  { handleOpenFullscreenEdit("situation-awareness", true); }
                }
                currentUser={{
                  name: currentUser.fullName ?? `${currentUser.firstName} ${currentUser.lastName}`,
                  initials: (currentUser.fullName ?? `${currentUser.firstName} ${currentUser.lastName}`).split(' ').map(n => n[0]).join('').toUpperCase(),
                  role: currentUser.roles?.join(', ') ?? ''
                }}
                assignedPhysician={currentPatientData.assignedPhysician}
              />
            </div>
          </div>

          {/* Right Column */}
          <div className="xl:col-span-1 space-y-6">
            {/* A - Action List */}
            <div className="sticky top-32">
              <div className="bg-white rounded-lg border border-gray-100">
                <div className="p-4 border-b border-gray-100">
                  <div className="flex items-center space-x-3">
                    <div className="w-8 h-8 bg-blue-100 rounded-full flex items-center justify-center">
                      <span className="font-bold text-blue-700">A</span>
                    </div>
                    <div className="flex-1">
                      <h3 className="font-medium text-gray-900">
                        {t("mainContent:sections.actionList")}
                      </h3>
                      <p className="text-sm text-gray-600">
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
                                  <span className="text-gray-400 mt-0.5">
                                    •
                                  </span>
                                  <span>{point}</span>
                                </li>
                              ),
                            )}
                          </ul>
                        </div>
                      </TooltipContent>
                    </Tooltip>
                  </div>
                </div>
                <div className="p-6">
                  <ActionList
                    compact
                    expanded
                    collaborators={activeCollaborators}
                    onOpenThread={handleOpenDiscussion}
                    handoverId={handoverData?.id}
                    currentUser={currentUser}
                    assignedPhysician={currentUser}
                  />
                </div>
              </div>

              {/* S - Synthesis by Receiver */}
              <div className="bg-white rounded-lg border border-gray-100 mt-6">
                <div className="p-4 border-b border-gray-100">
                  <div className="flex items-center space-x-3">
                    <div className="w-8 h-8 bg-purple-100 rounded-full flex items-center justify-center">
                      <span className="font-bold text-purple-700">S</span>
                    </div>
                    <div className="flex-1">
                      <h3 className="font-medium text-gray-900">
                        {t("mainContent:sections.synthesisByReceiver")}
                      </h3>
                      <p className="text-sm text-gray-600">
                        {t("mainContent:sections.synthesisByReceiverDescription")}
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
                                  <span className="text-gray-400 mt-0.5">
                                    •
                                  </span>
                                  <span>{point}</span>
                                </li>
                              ),
                            )}
                          </ul>
                        </div>
                      </TooltipContent>
                    </Tooltip>
                  </div>
                </div>
                <div className="p-6">
                  <SynthesisByReceiver
                    currentUser={currentUser}
                    receivingPhysician={currentPatientData.receivingPhysician}
                    onComplete={setHandoverComplete}
                    onOpenThread={handleOpenDiscussion}
                  />
                </div>
              </div>
            </div>
          </div>
        </div>
      ) : null}

      {/* Single Column Layout - Subtle Borders & I-PASS Guidelines */}
      <div
        className={`space-y-3 ${layoutMode === "columns" ? "xl:hidden" : ""}`}
      >
        {/* I - Illness Severity */}
        <Collapsible asChild>
          <div className="bg-white rounded-lg border border-gray-100 overflow-hidden">
            <CollapsibleTrigger asChild>
              <div className="p-4 bg-white border-b border-gray-100 cursor-pointer hover:bg-gray-50 transition-colors">
                <div className="flex items-center justify-between">
                  <div className="flex items-center space-x-4">
                    <div className="w-8 h-8 bg-blue-100 rounded-full flex items-center justify-center">
                      <span className="font-bold text-blue-700">I</span>
                    </div>
                    <div>
                      <div className="flex items-center space-x-2">
                        <h3 className="font-semibold text-gray-900">
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
                                      <span className="text-gray-400 mt-0.5">
                                        •
                                      </span>
                                      <span>{point}</span>
                                    </li>
                                  ),
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
            </CollapsibleTrigger>
            <CollapsibleContent>
              <div className="p-6">
                <IllnessSeverity
                  assignedPhysician={currentPatientData.assignedPhysician}
                  currentUser={currentUser}
                />
              </div>
            </CollapsibleContent>
          </div>
        </Collapsible>

        {/* P - Patient Summary */}
        <Collapsible asChild>
          <div className="bg-white rounded-lg border border-gray-100 overflow-hidden">
            <CollapsibleTrigger asChild>
              <div className="p-4 bg-white border-b border-gray-100 cursor-pointer hover:bg-gray-50 transition-colors">
                <div className="flex items-center justify-between">
                  <div className="flex items-center space-x-4">
                    <div className="w-8 h-8 bg-blue-100 rounded-full flex items-center justify-center">
                      <span className="font-bold text-blue-700">P</span>
                    </div>
                    <div>
                      <div className="flex items-center space-x-2">
                        <h3 className="font-semibold text-gray-900">
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
                                      <span className="text-gray-400 mt-0.5">
                                        •
                                      </span>
                                      <span>{point}</span>
                                    </li>
                                  ),
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
            </CollapsibleTrigger>
            <CollapsibleContent>
              <div className="p-6">
                <PatientSummary
                  patientId={currentPatientData.id}
                  assignedPhysician={currentPatientData.assignedPhysician}
                  currentUser={currentUser}
                  syncStatus={syncStatus}
                  onOpenThread={handleOpenDiscussion}
                  onSyncStatusChange={setSyncStatus}
                  onRequestFullscreen={() =>
                    { handleOpenFullscreenEdit("patient-summary"); }
                  }
                />
              </div>
            </CollapsibleContent>
          </div>
        </Collapsible>

        {/* A - Action List */}
        <Collapsible asChild>
          <div className="bg-white rounded-lg border border-gray-100 overflow-hidden">
            <CollapsibleTrigger asChild>
              <div className="p-4 bg-white border-b border-gray-100 cursor-pointer hover:bg-gray-50 transition-colors">
                <div className="flex items-center justify-between">
                  <div className="flex items-center space-x-4">
                    <div className="w-8 h-8 bg-blue-100 rounded-full flex items-center justify-center">
                      <span className="font-bold text-blue-700">A</span>
                    </div>
                    <div>
                      <div className="flex items-center space-x-2">
                        <h3 className="font-semibold text-gray-900">
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
                                      <span className="text-gray-400 mt-0.5">
                                        •
                                      </span>
                                      <span>{point}</span>
                                    </li>
                                  ),
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
            </CollapsibleTrigger>
            <CollapsibleContent>
              <div className="p-6">
                <ActionList
                  expanded
                  collaborators={activeCollaborators}
                  onOpenThread={handleOpenDiscussion}
                  handoverId={handoverData?.id}
                  currentUser={currentUser}
                  assignedPhysician={currentUser}
                />
              </div>
            </CollapsibleContent>
          </div>
        </Collapsible>

        {/* S - Current Situation */}
        <Collapsible asChild>
          <div className="bg-white rounded-lg border border-gray-100 overflow-hidden">
            <CollapsibleTrigger asChild>
              <div className="p-4 bg-white border-b border-gray-100 cursor-pointer hover:bg-gray-50 transition-colors">
                <div className="flex items-center justify-between">
                  <div className="flex items-center space-x-4">
                    <div className="w-8 h-8 bg-blue-100 rounded-full flex items-center justify-center">
                      <span className="font-bold text-blue-700">S</span>
                    </div>
                    <div>
                      <div className="flex items-center space-x-2">
                        <h3 className="font-semibold text-gray-900">
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
                                      <span className="text-gray-400 mt-0.5">
                                        •
                                      </span>
                                      <span>{point}</span>
                                    </li>
                                  ),
                                )}
                              </ul>
                            </div>
                          </TooltipContent>
                        </Tooltip>
                      </div>
                      <p className="text-sm text-gray-700">
                        {t("mainContent:sections.situationAwarenessDescription")}
                      </p>
                    </div>
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
            </CollapsibleTrigger>
            <CollapsibleContent>
              <SituationAwareness
                handoverId={handoverData.id}
                collaborators={activeCollaborators}
                syncStatus={syncStatus}
                onOpenThread={handleOpenDiscussion}
                onSyncStatusChange={setSyncStatus}
                onRequestFullscreen={() =>
                  { handleOpenFullscreenEdit("situation-awareness", true); }
                }
                currentUser={{
                  name: currentUser.fullName ?? `${currentUser.firstName} ${currentUser.lastName}`,
                  initials: (currentUser.fullName ?? `${currentUser.firstName} ${currentUser.lastName}`).split(' ').map(n => n[0]).join('').toUpperCase(),
                  role: currentUser.roles?.join(', ') ?? ''
                }}
                assignedPhysician={currentPatientData.assignedPhysician}
              />
            </CollapsibleContent>
          </div>
        </Collapsible>

        {/* S - Synthesis by Receiver */}
        <Collapsible asChild>
          <div className="bg-white rounded-lg border border-gray-100 overflow-hidden">
            <CollapsibleTrigger asChild>
              <div className="p-4 bg-white border-b border-gray-100 cursor-pointer hover:bg-gray-50 transition-colors">
                <div className="flex items-center justify-between">
                  <div className="flex items-center space-x-4">
                    <div className="w-8 h-8 bg-purple-100 rounded-full flex items-center justify-center">
                      <span className="font-bold text-purple-700">S</span>
                    </div>
                    <div>
                      <div className="flex items-center space-x-2">
                        <h3 className="font-semibold text-gray-900">
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
                                      <span className="text-gray-400 mt-0.5">
                                        •
                                      </span>
                                      <span>{point}</span>
                                    </li>
                                  ),
                                )}
                              </ul>
                            </div>
                          </TooltipContent>
                        </Tooltip>
                      </div>
                      <p className="text-sm text-gray-700">
                        {t("mainContent:sections.synthesisByReceiverDescription")}
                      </p>
                    </div>
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
            </CollapsibleTrigger>
            <CollapsibleContent>
              <div className="p-6">
                <SynthesisByReceiver
                  currentUser={currentUser}
                  receivingPhysician={currentPatientData.receivingPhysician}
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
