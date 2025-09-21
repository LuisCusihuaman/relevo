import type { SyncStatus } from "@/common/types";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { ScrollArea } from "@/components/ui/scroll-area";
import { Textarea } from "@/components/ui/textarea";
import {
  Activity,
  AlertCircle,
  Edit,
  Plus,
  Send,
  Trash2,
  Users,
  X,
} from "lucide-react";
import { useEffect, useState } from "react";
import type { JSX } from "react";
import { useTranslation } from "react-i18next";

// Enhanced collaborator with typing indicators
interface Collaborator {
  id: number;
  name: string;
  initials: string;
  color: string;
  role: string;
  isTyping?: boolean;
  lastSeen?: string;
}

interface ContingencyPlan {
  id: number;
  condition: string;
  action: string;
  priority: "low" | "medium" | "high";
  status: "active" | "planned";
  submittedBy: string;
  submittedTime: string;
  submittedDate: string;
}

interface SituationAwarenessProps {
  collaborators?: Array<Collaborator>;
  onOpenThread?: (section: string) => void;
  focusMode?: boolean;
  fullscreenMode?: boolean;
  autoEdit?: boolean;
  onRequestFullscreen?: () => void;
  hideControls?: boolean; // NEW PROP to hide internal save/done buttons
  onSave?: () => void; // Handler for external save button
  syncStatus?: SyncStatus;
  onSyncStatusChange?: (status: SyncStatus) => void;
  currentUser?: {
    name: string;
    initials: string;
    role: string;
  };
  assignedPhysician?: {
    name: string;
    initials: string;
    role: string;
  };
  onContentChange?: () => void;
}

export function SituationAwareness({
  collaborators: _collaborators = [],
  onOpenThread: _onOpenThread,
  focusMode = false,
  fullscreenMode = false,
  autoEdit = false,
  onRequestFullscreen,
  hideControls = false, // Default to false for backwards compatibility
  onSave: _onSave, // External save handler
  syncStatus: _syncStatus = "synced",
  onSyncStatusChange: _onSyncStatusChange,
  currentUser = { name: "Dr. Johnson", initials: "DJ", role: "Day Attending" },
  assignedPhysician = {
    name: "Dr. Johnson",
    initials: "DJ",
    role: "Day Attending",
  },
  onContentChange,
}: SituationAwarenessProps): JSX.Element {
  const { t } = useTranslation("situationAwareness");

  // Live situation documentation (collaborative)
  const [currentSituation, setCurrentSituation] = useState(
    t("initialSituation.header") +
      "\n" +
      t("initialSituation.line1") +
      "\n" +
      t("initialSituation.line2") +
      "\n" +
      t("initialSituation.line3") +
      "\n" +
      t("initialSituation.line4") +
      "\n" +
      t("initialSituation.line5") +
      "\n\n" +
      t("initialSituation.monitoringHeader") +
      "\n" +
      t("initialSituation.monitoring1") +
      "\n" +
      t("initialSituation.monitoring2") +
      "\n" +
      t("initialSituation.monitoring3") +
      "\n" +
      t("initialSituation.monitoring4") +
      "\n" +
      t("initialSituation.monitoring5") +
      "\n\n" +
      t("initialSituation.teamNotesHeader") +
      "\n" +
      t("initialSituation.teamNote1") +
      "\n" +
      t("initialSituation.teamNote2") +
      "\n" +
      t("initialSituation.teamNote3") +
      "\n" +
      t("initialSituation.teamNote4") +
      "\n\n" +
      t("initialSituation.nextShiftGoalsHeader") +
      "\n" +
      t("initialSituation.goal1") +
      "\n" +
      t("initialSituation.goal2") +
      "\n" +
      t("initialSituation.goal3") +
      "\n" +
      t("initialSituation.goal4"),
  );

  // State for editing mode
  const [isEditing, setIsEditing] = useState(false);
  const [_lastEdit, setLastEdit] = useState(new Date());
  const [autoSaveStatus, setAutoSaveStatus] = useState<
    "saved" | "saving" | "error"
  >("saved");

  // Auto-start editing when in fullscreen with autoEdit
  useEffect(() => {
    if (fullscreenMode && autoEdit) {
      setIsEditing(true);
    }
  }, [fullscreenMode, autoEdit]);

  // Contingency plans
  const [contingencyPlans, setContingencyPlans] = useState<Array<ContingencyPlan>>(
    () => [
      {
        id: 1,
        condition: t("contingencyPlans.plan1.condition"),
        action: t("contingencyPlans.plan1.action"),
        priority: "high",
        status: "active",
        submittedBy: t("doctors.johnson"),
        submittedTime: "14:20",
        submittedDate: t("time.today"),
      },
      {
        id: 2,
        condition: t("contingencyPlans.plan2.condition"),
        action: t("contingencyPlans.plan2.action"),
        priority: "medium",
        status: "active",
        submittedBy: t("doctors.martinez"),
        submittedTime: "15:45",
        submittedDate: t("time.today"),
      },
      {
        id: 3,
        condition: t("contingencyPlans.plan3.condition"),
        action: t("contingencyPlans.plan3.action"),
        priority: "high",
        status: "planned",
        submittedBy: t("doctors.rodriguez"),
        submittedTime: "16:10",
        submittedDate: t("time.today"),
      },
    ],
  );

  // New plan form state
  const [showNewPlanForm, setShowNewPlanForm] = useState(false);
  const [newPlan, setNewPlan] = useState({
    condition: "",
    action: "",
    priority: "medium",
  });
  const [isSubmitting, setIsSubmitting] = useState(false);

  // Check if current user can delete plans (only assigned physician)
  const canDeletePlans = currentUser.name === assignedPhysician.name;

  // Handle situation documentation changes with auto-save simulation
  const handleSituationChange = (value: string) => {
    setCurrentSituation(value);
    setLastEdit(new Date());
    setAutoSaveStatus("saving");
    if (onContentChange) {
      onContentChange();
    }

    // Simulate auto-save after user stops typing
    setTimeout(() => {
      setAutoSaveStatus("saved");
    }, 1000);
  };

  // Submit new contingency plan
  const handleSubmitPlan = () => {
    if (!newPlan.condition || !newPlan.action) return;

    setIsSubmitting(true);

    // Simulate API call
    setTimeout(() => {
      const plan: ContingencyPlan = {
        id: Date.now(),
        condition: newPlan.condition.trim(),
        action: newPlan.action.trim(),
        priority: newPlan.priority as "low" | "medium" | "high",
        status: "active",
        submittedBy: currentUser.name,
        submittedTime: new Date().toLocaleTimeString([], {
          hour: "2-digit",
          minute: "2-digit",
        }),
        submittedDate: t("time.today"),
      };

      setContingencyPlans((previous) => [...previous, plan]);
      setNewPlan({ condition: "", action: "", priority: "medium" });
      setShowNewPlanForm(false);
      setIsSubmitting(false);
    }, 500);
  };

  // Delete contingency plan (only assigned physician can delete)
  const handleDeletePlan = (planId: number) => {
    if (!canDeletePlans) return;
    setContingencyPlans((previous) => previous.filter((plan) => plan.id !== planId));
  };

  // Handle keyboard shortcuts
  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === "Enter" && (e.metaKey || e.ctrlKey)) {
      e.preventDefault();
      handleSubmitPlan();
    }
  };

  // Handle click for editing or fullscreen - SIMPLIFIED FOR SINGLE CLICK
  const handleClick = () => {
    if (focusMode) return;

    if (fullscreenMode) {
      // If in fullscreen, just start editing
      setIsEditing(true);
    } else if (onRequestFullscreen) {
      // If not in fullscreen, go to fullscreen with auto-edit
      onRequestFullscreen();
    } else {
      // Fallback to regular editing
      setIsEditing(true);
    }
  };

  // Priority colors
  const getPriorityColor = (priority: string) => {
    switch (priority) {
      case "high":
        return "text-red-600 border-red-200";
      case "medium":
        return "text-amber-600 border-amber-200";
      case "low":
        return "text-emerald-600 border-emerald-200";
      default:
        return "text-gray-600 border-gray-200";
    }
  };

  // Status badge styling
  const getStatusBadge = (status: string) => {
    switch (status) {
      case "active":
        return "bg-emerald-50 text-emerald-700 border-emerald-200";
      case "planned":
        return "bg-blue-50 text-blue-700 border-blue-200";
      default:
        return "bg-gray-50 text-gray-700 border-gray-200";
    }
  };

  // Optimized height for fullscreen
  const contentHeight = fullscreenMode ? "min-h-[60vh]" : "h-80";

  return (
    <div className="space-y-0">
      {/* Live Situation Documentation - Optimized Border Radius */}
      <div className="space-y-4">
        {isEditing ? (
          /* Editing Mode - No top border radius */
          <div className="relative">
            <div
              className={`bg-white border-2 border-blue-200 ${fullscreenMode ? "rounded-lg" : "rounded-t-none rounded-b-none"}`}
            >
              {/* Enhanced Editor Header with top rounded corners */}
              <div
                className={`flex items-center justify-between px-6 py-4 border-b border-gray-100 bg-blue-25/50 ${fullscreenMode ? "rounded-t-lg" : "rounded-t-lg"}`}
              >
                <div className="flex items-center space-x-3">
                  <Edit className="w-5 h-5 text-blue-600" />
                  <h4 className="text-lg font-medium text-blue-800">
                    {fullscreenMode
                      ? t("editor.fullscreenTitle")
                      : t("editor.title")}
                  </h4>
                </div>
                <div className="flex items-center space-x-3">
                  <div className="flex items-center space-x-1">
                    <div
                      className={`w-2 h-2 rounded-full ${
                        autoSaveStatus === "saved"
                          ? "bg-green-500"
                          : autoSaveStatus === "saving"
                            ? "bg-amber-500 animate-pulse"
                            : "bg-red-500"
                      }`}
                    ></div>
                    <span className="text-sm text-blue-600">
                      {autoSaveStatus === "saved"
                        ? t("autoSave.saved")
                        : autoSaveStatus === "saving"
                          ? t("autoSave.saving")
                          : t("autoSave.error")}
                    </span>
                  </div>
                  {/* ONLY SHOW DONE BUTTON IF NOT HIDING CONTROLS */}
                  {!hideControls && (
                    <Button
                      className="text-xs text-blue-600 hover:bg-blue-100 h-7 px-2"
                      size="sm"
                      variant="ghost"
                      onClick={() => { setIsEditing(false); }}
                    >
                      {t("done")}
                    </Button>
                  )}
                </div>
              </div>

              {/* Fixed Height Document Content Area - no border radius */}
              <div className={`relative ${contentHeight}`}>
                <ScrollArea className="h-full">
                  <div className="p-6">
                    <Textarea
                      className={`w-full h-full ${fullscreenMode ? "min-h-[60vh]" : "min-h-[400px]"} border-0 bg-transparent p-4 resize-none text-gray-900 leading-relaxed placeholder:text-gray-400 focus:outline-none focus:ring-0 focus:ring-offset-0 focus-visible:ring-0 focus-visible:ring-offset-0 rounded-none`}
                      placeholder={t("editor.placeholder")}
                      value={currentSituation}
                      style={{
                        fontFamily: "system-ui, -apple-system, sans-serif",
                        fontSize: fullscreenMode ? "16px" : "14px",
                        lineHeight: "1.6",
                        background: "transparent !important",
                      }}
                      onChange={(e) => { handleSituationChange(e.target.value); }}
                    />
                  </div>
                </ScrollArea>
              </div>

              {/* Auto-Save Status Footer - no bottom rounded corners */}
              <div
                className={`flex items-center justify-between px-4 py-2 border-t border-gray-100 bg-gray-25/30 ${fullscreenMode ? "rounded-b-lg" : ""}`}
              >
                <div className="flex items-center space-x-3 text-xs text-gray-500">
                  <span>
                    {currentSituation.split("\n").length} {t("editor.lines")}
                  </span>
                  <span>
                    {currentSituation.split(" ").length} {t("editor.words")}
                  </span>
                  {!hideControls && <span>{t("editor.autosaving")}</span>}
                  {hideControls && (
                    <span>{t("editor.useFullscreenControls")}</span>
                  )}
                </div>
                <span className="text-xs text-gray-500">
                  {t("editor.peopleEditing", { count: 3 })}
                </span>
              </div>
            </div>
          </div>
        ) : (
          /* View Mode - No top border radius */
          <div
            className="relative group"
            role={!focusMode ? "button" : undefined}
            tabIndex={!focusMode ? 0 : undefined}
            aria-label={
              !focusMode ? t("view.editAriaLabel") : undefined
            }
            onClick={handleClick}
            onKeyDown={(e) => {
              if ((e.key === "Enter" || e.key === " ") && !focusMode) {
                e.preventDefault();
                handleClick();
              }
            }}
          >
            <div
              className={`bg-white ${fullscreenMode ? "rounded-lg" : "rounded-t-none rounded-b-none"} transition-all duration-200 ${!focusMode ? "cursor-pointer" : ""}`}
            >
              {/* Enhanced Header with top rounded corners */}
              <div
                className={`flex items-center justify-between px-6 py-4 border-b border-gray-100 bg-blue-25/50 ${fullscreenMode ? "rounded-t-lg" : "rounded-t-lg"}`}
              >
                <div className="flex items-center space-x-3">
                  <Activity className="w-5 h-5 text-blue-600" />
                  <div className="flex items-center space-x-3">
                    <h4 className="text-lg font-medium text-blue-700">
                      {fullscreenMode
                        ? t("view.fullscreenTitle")
                        : t("currentSituation.title")}
                    </h4>
                    <Badge
                      className="text-xs bg-blue-50 text-blue-700 border-blue-200"
                      variant="outline"
                    >
                      <Users className="w-3 h-3 mr-1" />
                      {t("view.allCanEdit")}
                    </Badge>
                  </div>
                </div>
                <div className="flex items-center space-x-2">
                  <div className="w-2 h-2 bg-green-500 rounded-full"></div>
                  <span className="text-sm text-gray-500">{t("view.active")}</span>
                </div>
              </div>

              {/* Fixed Height Document Content - no border radius */}
              <div className={`relative ${contentHeight}`}>
                <ScrollArea className="h-full">
                  <div className="p-6">
                    <div
                      className="text-gray-900 leading-relaxed whitespace-pre-line"
                      style={{
                        fontFamily: "system-ui, -apple-system, sans-serif",
                        fontSize: fullscreenMode ? "16px" : "14px",
                        lineHeight: "1.6",
                      }}
                    >
                      {currentSituation}
                    </div>
                  </div>
                </ScrollArea>
              </div>

              {/* Document Footer - no bottom rounded corners */}
              <div
                className={`flex items-center justify-between px-4 py-2 border-t border-gray-100 bg-gray-25/30 ${fullscreenMode ? "rounded-b-lg" : ""}`}
              >
                <div className="flex items-center space-x-3 text-xs text-gray-500">
                  <span>
                    {currentSituation.split("\n").length} {t("editor.lines")}
                  </span>
                  <span>
                    {currentSituation.split(" ").length} {t("editor.words")}
                  </span>
                  <span>{t("view.lastUpdatedBy", { user: "Dr. Rodriguez" })}</span>
                </div>
                {!focusMode && (
                  <div className="opacity-0 group-hover:opacity-100 transition-opacity">
                    <div className="flex items-center space-x-1 text-xs text-gray-500">
                      <Edit className="w-3 h-3" />
                      <span>{t("view.clickToEdit")}</span>
                    </div>
                  </div>
                )}
              </div>
            </div>
          </div>
        )}
      </div>

      {/* Separator integrated as border - no gap */}
      {!fullscreenMode && <div className="border-t border-gray-200"></div>}

      {/* Submitted Contingency Plans - Clean Final Display */}
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

          {/* Clean Submitted Plans List */}
          <div className="space-y-3">
            {contingencyPlans.map((plan) => (
              <div
                key={plan.id}
                className="p-4 rounded-lg border border-gray-200 bg-white hover:border-gray-300 transition-all group"
              >
                <div className="space-y-3">
                  {/* Plan Header */}
                  <div className="flex items-start justify-between">
                    <div className="flex items-start space-x-3 flex-1 min-w-0">
                      <div
                        className={`text-xs px-2 py-1 rounded border font-medium ${getPriorityColor(plan.priority)}`}
                      >
                        {plan.priority.toUpperCase()}
                      </div>
                      <div className="flex-1 min-w-0">
                        <Badge
                          className={`text-xs border ${getStatusBadge(plan.status)} mb-2`}
                        >
                          {plan.status === "active"
                            ? t("status.active")
                            : t("status.planned")}
                        </Badge>
                      </div>
                    </div>

                    {/* Delete button - Only for assigned physician */}
                    {!focusMode && canDeletePlans && (
                      <Button
                        className="opacity-0 group-hover:opacity-100 transition-opacity text-red-600 hover:text-red-700 hover:bg-red-50 h-6 w-6 p-0"
                        size="sm"
                        variant="ghost"
                        onClick={(e) => {
                          e.stopPropagation();
                          handleDeletePlan(plan.id);
                        }}
                      >
                        <Trash2 className="w-3 h-3" />
                      </Button>
                    )}
                  </div>

                  {/* Clean IF/THEN Content - Final Version */}
                  <div className="space-y-2">
                    <div className="flex items-start space-x-2">
                      <span className="text-sm font-medium text-gray-700 flex-shrink-0">
                        {t("contingencyPlanning.ifPrefix")}
                      </span>
                      <span className="text-sm text-gray-900">
                        {plan.condition}
                      </span>
                    </div>
                    <div className="flex items-start space-x-2">
                      <span className="text-sm font-medium text-gray-700 flex-shrink-0">
                        {t("contingencyPlanning.thenPrefix")}
                      </span>
                      <span className="text-sm text-gray-900">
                        {plan.action}
                      </span>
                    </div>
                  </div>

                  {/* Plan Footer - Submitted info */}
                  <div className="flex items-center justify-between text-xs text-gray-500 pt-2 border-t border-gray-100">
                    <span>
                      {t("submittedBy", {
                        name: plan.submittedBy,
                        time: plan.submittedTime,
                        date: plan.submittedDate,
                      })}
                    </span>
                    <span>{plan.submittedDate}</span>
                  </div>
                </div>
              </div>
            ))}
          </div>

          {/* Add New Plan - Submit-Based Workflow */}
          {!focusMode && !showNewPlanForm ? (
            <Button
              className="w-full text-gray-600 border-gray-200 hover:bg-gray-50"
              variant="outline"
              onClick={() => { setShowNewPlanForm(true); }}
            >
              <Plus className="w-4 h-4 mr-2" />
              {t("contingencyPlanning.addPlan")}
            </Button>
          ) : (
            !focusMode && (
              <div className="p-4 border border-gray-200 rounded-lg bg-gray-25">
                <div className="space-y-4">
                  <div className="flex items-center justify-between">
                    <h5 className="font-medium text-gray-900">
                      {t("newPlan.title")}
                    </h5>
                    <Button
                      className="h-6 w-6 p-0"
                      disabled={isSubmitting}
                      size="sm"
                      variant="ghost"
                      onClick={() => { setShowNewPlanForm(false); }}
                    >
                      <X className="w-4 h-4" />
                    </Button>
                  </div>

                  <div className="space-y-3">
                    <div>
                      <label
                        className="text-sm font-medium text-gray-700 mb-1 block"
                        htmlFor="plan-condition"
                      >
                        {t("contingencyPlanning.form.conditionLabel")}
                      </label>
                      <Textarea
                        className="w-full p-2 border border-gray-300 rounded-md text-sm bg-white"
                        id="plan-condition"
                        rows={2}
                        value={newPlan.condition}
                        placeholder={t(
                          "contingencyPlanning.form.conditionPlaceholder",
                        )}
                        onKeyDown={handleKeyDown}
                        onChange={(e) =>
                          { setNewPlan({ ...newPlan, condition: e.target.value }); }
                        }
                      />
                    </div>

                    <div>
                      <label
                        className="text-sm font-medium text-gray-700 mb-1 block"
                        htmlFor="plan-action"
                      >
                        {t("contingencyPlanning.form.actionLabel")}
                      </label>
                      <Textarea
                        className="min-h-[60px] border-gray-300 focus:border-blue-400 focus:ring-blue-100 bg-white"
                        disabled={isSubmitting}
                        id="plan-action"
                        value={newPlan.action}
                        placeholder={t(
                          "contingencyPlanning.form.actionPlaceholder",
                        )}
                        onKeyDown={handleKeyDown}
                        onChange={(e) =>
                          { setNewPlan({ ...newPlan, action: e.target.value }); }
                        }
                      />
                    </div>

                    <div>
                      <label
                        className="text-sm font-medium text-gray-700 mb-1 block"
                        htmlFor="plan-priority"
                      >
                        {t("contingencyPlanning.form.priorityLabel")}
                      </label>
                      <select
                        className="w-full p-2 text-sm border border-gray-300 rounded-lg bg-white focus:border-blue-400 focus:ring-blue-100"
                        disabled={isSubmitting}
                        id="plan-priority"
                        value={newPlan.priority}
                        onChange={(e) =>
                          { setNewPlan({ ...newPlan, priority: e.target.value }); }
                        }
                      >
                        <option value="low">{t("priorities.low")}</option>
                        <option value="medium">{t("priorities.medium")}</option>
                        <option value="high">{t("priorities.high")}</option>
                      </select>
                    </div>
                  </div>

                  <div className="flex justify-end space-x-2">
                    <Button
                      className="text-xs border-gray-300 hover:bg-gray-50"
                      disabled={isSubmitting}
                      size="sm"
                      variant="outline"
                      onClick={() => { setShowNewPlanForm(false); }}
                    >
                      {t("cancel")}
                    </Button>
                    <Button
                      className="text-xs bg-blue-600 hover:bg-blue-700 text-white"
                      size="sm"
                      disabled={
                        !newPlan.condition || !newPlan.action || isSubmitting
                      }
                      onClick={handleSubmitPlan}
                    >
                      {isSubmitting ? (
                        <>
                          <div className="w-3 h-3 border border-white border-t-transparent rounded-full animate-spin mr-1"></div>
                          {t("newPlan.submitting")}
                        </>
                      ) : (
                        <>
                          <Send className="w-3 h-3 mr-1" />
                          {t("newPlan.submit")}
                        </>
                      )}
                    </Button>
                  </div>
                </div>
              </div>
            )
          )}
        </div>
      )}
    </div>
  );
}
