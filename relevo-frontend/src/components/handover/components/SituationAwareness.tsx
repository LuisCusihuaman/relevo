import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { AlertCircle, Plus } from "lucide-react";
import { useCallback, useEffect, useState, type JSX } from "react";
import { useTranslation } from "react-i18next";
import {
  useSituationAwareness,
  useUpdateSituationAwareness,
  useHandoverContingencyPlans,
  useCreateContingencyPlan,
  useDeleteContingencyPlan
} from "@/api/endpoints/handovers";
import type { ContingencyPlan } from "@/types/domain";
import { SituationEditor } from "./situation-awareness/SituationEditor";
import { ContingencyPlanList } from "./situation-awareness/ContingencyPlanList";
import { ContingencyPlanForm, type NewPlanData } from "./situation-awareness/ContingencyPlanForm";

interface SituationAwarenessProps {
  handoverId: string;
  currentUser: {
    name: string;
    initials: string;
    role: string;
  };
  assignedPhysician?: {
    name: string;
    initials: string;
    role: string;
  };
  autoEdit?: boolean;
  fullscreenMode?: boolean;
  hideControls?: boolean;
  onContentChange?: () => void;
  onRequestFullscreen?: () => void;
}

export function SituationAwareness({
  handoverId,
  currentUser,
  assignedPhysician,
  autoEdit = false,
  fullscreenMode = false,
  hideControls = false,
  onContentChange,
  onRequestFullscreen,
}: SituationAwarenessProps): JSX.Element {
  const { t } = useTranslation("situationAwareness");

  // Determine if current user can edit (typically the assigned physician can edit)
  const canEdit = assignedPhysician ? currentUser?.name === assignedPhysician?.name : true;

  // Fetch situation awareness data
  const { data: situationData, isLoading: isLoadingSituation } = useSituationAwareness(handoverId);
  const { data: contingencyData, isLoading: isLoadingContingency } = useHandoverContingencyPlans(handoverId);

  // Mutations
  const updateSituationMutation = useUpdateSituationAwareness();
  const createContingencyMutation = useCreateContingencyPlan();
  const deleteContingencyMutation = useDeleteContingencyPlan();

  // Derive initial editing state directly from props
  const initialEditingState = (fullscreenMode && autoEdit && canEdit) || false;

  // Local state for situation awareness content
  const [isEditing, setIsEditing] = useState(initialEditingState);
  const [autoSaveStatus, setAutoSaveStatus] = useState<"saved" | "saving" | "error">("saved");

  // Build default template text
  const getDefaultSituationText = useCallback((): string => {
    const sections = [
      [t("initialSituation.header"), t("initialSituation.line1"), t("initialSituation.line2"), t("initialSituation.line3"), t("initialSituation.line4"), t("initialSituation.line5")],
      [t("initialSituation.monitoringHeader"), t("initialSituation.monitoring1"), t("initialSituation.monitoring2"), t("initialSituation.monitoring3"), t("initialSituation.monitoring4"), t("initialSituation.monitoring5")],
      [t("initialSituation.teamNotesHeader"), t("initialSituation.teamNote1"), t("initialSituation.teamNote2"), t("initialSituation.teamNote3"), t("initialSituation.teamNote4")],
      [t("initialSituation.nextShiftGoalsHeader"), t("initialSituation.goal1"), t("initialSituation.goal2"), t("initialSituation.goal3"), t("initialSituation.goal4")],
    ];
    return sections.map(section => section.join("\n")).join("\n\n");
  }, [t]);

  // Derived state: what to show
  const serverContent = situationData?.situationAwareness?.content;
  const defaultContent = getDefaultSituationText();
  const [draft, setDraft] = useState(serverContent || defaultContent);

  // Sync draft with server content only when not editing (or initially)
  useEffect(() => {
    if (!isEditing && serverContent) {
      setDraft(serverContent);
    } else if (!isEditing && !serverContent) {
        setDraft(defaultContent);
    }
  }, [serverContent, defaultContent, isEditing]);

  const displayContent = isEditing ? draft : (serverContent || defaultContent);

  // Convert API contingency plans to component format
  const contingencyPlans: Array<ContingencyPlan> = contingencyData || [];

  // New plan form state
  const [showNewPlanForm, setShowNewPlanForm] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);

  // Check if current user can delete plans (only assigned physician)
  const canDeletePlans = assignedPhysician ? currentUser.name === assignedPhysician.name : false;

  // Handle situation documentation changes with real auto-save
  const handleSituationChange = async (value: string): Promise<void> => {
    setDraft(value);
    setAutoSaveStatus("saving");
    onContentChange?.();

    try {
      await updateSituationMutation.mutateAsync({
        handoverId,
        content: value,
        status: (situationData?.situationAwareness?.status as any) || "Draft",
      });
      setAutoSaveStatus("saved");
    } catch (error) {
      console.error("Failed to save situation awareness:", error);
      setAutoSaveStatus("error");
    }
  };

  // Submit new contingency plan
  const handleSubmitPlan = async (data: NewPlanData): Promise<void> => {
    setIsSubmitting(true);

    try {
      await createContingencyMutation.mutateAsync({
        handoverId,
        conditionText: data.condition.trim(),
        actionText: data.action.trim(),
        priority: data.priority,
      });

      setShowNewPlanForm(false);
    } catch (error) {
      console.error("Failed to create contingency plan:", error);
    } finally {
      setIsSubmitting(false);
    }
  };

  // Delete contingency plan (only assigned physician can delete)
  const handleDeletePlan = (planId: string): void => {
    if (!canDeletePlans) return;

    deleteContingencyMutation.mutate({
      handoverId,
      contingencyId: planId,
    });
  };

  // Handle click for editing or fullscreen
  const handleEnterEdit = (): void => {
    if (!isEditing) {
      // Initialize draft with current content if needed
      if (draft !== displayContent) setDraft(displayContent);
      setIsEditing(true);
    }
    
    if (fullscreenMode) {
      // Already in fullscreen, just ensure editing is on (handled above)
    } else if (onRequestFullscreen) {
      onRequestFullscreen();
    }
  };

  // Show loading state
  if (isLoadingSituation || isLoadingContingency) {
    return (
      <div className="space-y-0">
        <div className="space-y-4">
          <div className="bg-white border border-gray-200 rounded-t-none rounded-b-none">
            <div className="px-6 py-4 border-b border-gray-100 bg-blue-25/50">
              <div className="flex items-center space-x-3">
                <div className="w-5 h-5 bg-gray-200 rounded"></div>
                <div className="h-5 bg-gray-200 rounded w-48"></div>
              </div>
            </div>
            <div className="p-6">
              <div className="animate-pulse space-y-2">
                <div className="h-4 bg-gray-200 rounded w-full"></div>
                <div className="h-4 bg-gray-200 rounded w-3/4"></div>
                <div className="h-4 bg-gray-200 rounded w-1/2"></div>
              </div>
            </div>
          </div>
        </div>

        {!fullscreenMode && (
          <div className="space-y-4 pt-6 px-6">
            <div className="animate-pulse space-y-3">
              <div className="h-6 bg-gray-200 rounded w-48"></div>
              <div className="h-32 bg-gray-200 rounded"></div>
              <div className="h-32 bg-gray-200 rounded"></div>
            </div>
          </div>
        )}
      </div>
    );
  }

  return (
    <div className="space-y-0">
      <div className="space-y-4">
        <SituationEditor
          autoSaveStatus={autoSaveStatus}
          canEdit={canEdit}
          content={displayContent}
          fullscreenMode={fullscreenMode}
          hideControls={hideControls}
          isEditing={isEditing || (fullscreenMode && autoEdit)}
          onChange={(value) => { void handleSituationChange(value); }}
          onEditChange={setIsEditing}
          onEnterEdit={handleEnterEdit}
        />
      </div>

      {!fullscreenMode && <div className="border-t border-gray-200"></div>}

      {!fullscreenMode && (
        <div className="space-y-4 pt-6 px-6">
          <div className="flex items-center justify-between">
            <div className="flex items-center space-x-2">
              <AlertCircle className="w-4 h-4 text-gray-600" />
              <h4 className="font-medium text-gray-900">
                {t("contingencyPlanning.title")}
              </h4>
            </div>
            <div className="flex items-center space-x-2">
              <Badge className="text-xs" variant="outline">
                {contingencyPlans.filter((p) => p.status === "active").length}{" "}
                {t("status.active")}
              </Badge>
              <Badge className="text-xs" variant="outline">
                {contingencyPlans.filter((p) => p.status === "planned").length}{" "}
                {t("status.planned")}
              </Badge>
            </div>
          </div>

          <ContingencyPlanList
            canDelete={canDeletePlans}
            plans={contingencyPlans}
            onDelete={handleDeletePlan}
          />

          {!showNewPlanForm ? (
            <Button
              className="w-full text-gray-600 border-gray-200 hover:bg-gray-50"
              variant="outline"
              onClick={() => { setShowNewPlanForm(true); }}
            >
              <Plus className="w-4 h-4 mr-2" />
              {t("contingencyPlanning.addPlan")}
            </Button>
          ) : (
            <ContingencyPlanForm
              isSubmitting={isSubmitting}
              onCancel={() => { setShowNewPlanForm(false); }}
              onSubmit={handleSubmitPlan}
            />
          )}
        </div>
      )}
    </div>
  );
}
