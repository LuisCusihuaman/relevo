import type { PatientHandoverData } from "@/types/domain";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { ScrollArea } from "@/components/ui/scroll-area";
import { Textarea } from "@/components/ui/textarea";
import { Clock, Edit, FileText, Lock, Save, Shield } from "lucide-react";
import { useCallback, useEffect, useState, forwardRef, useImperativeHandle } from "react";
import { useTranslation } from "react-i18next";
import {
	useUpdatePatientData,
} from "@/api/endpoints/handovers";

export interface PatientSummaryHandle {
  save: () => Promise<void>;
}

interface PatientSummaryProps {
  handoverId: string; // Required: ID of the handover
  patientData?: PatientHandoverData; // Patient data including summary text
  fullscreenMode?: boolean;
  autoEdit?: boolean;
  onRequestFullscreen?: () => void;
  hideControls?: boolean; // NEW PROP to hide internal save/done buttons
  currentUser?: {
    id: string;
    name: string;
    initials: string;
    role: string;
  };
  responsiblePhysician?: {
    id?: string;
    name: string;
  };
  handoverStateName?: "Draft" | "Ready" | "InProgress" | "Completed" | "Cancelled";
  onContentChange?: () => void;
  onSaveSuccess?: () => void;
}

export const PatientSummary = forwardRef<PatientSummaryHandle, PatientSummaryProps>(({
  handoverId,
  patientData: patientDataProp,
  fullscreenMode = false,
  autoEdit = false,
  onRequestFullscreen,
  hideControls = false, // Default to false for backwards compatibility
  currentUser,
  responsiblePhysician,
  handoverStateName,
  onContentChange,
  onSaveSuccess,
}, ref) => {
  const { t } = useTranslation("patientSummary");

  // Patient data now comes as prop instead of separate API call

  // Local state for editing
  const [isEditing, setIsEditing] = useState(false);
  const [isUpdating, setIsUpdating] = useState(false);
  const [editingText, setEditingText] = useState("");

  // Get the current summary text, defaulting to empty string if no data
  const displayText = patientDataProp?.summaryText || "";
  const currentText = isEditing ? editingText : displayText;

  // Check if current user can edit (only responsible physician for drafts)
  // If state is undefined/empty, treat as Draft (new handover)
  const effectiveState = handoverStateName || "Draft";
  
  // Check permissions: user can edit if:
  // 1. State allows editing (Draft, Ready, or InProgress)
  // 2. User ID matches responsible physician ID (primary check)
  // 3. OR if IDs are not available, check by name (fallback)
  const stateAllowsEditing = effectiveState === "Draft" || effectiveState === "Ready" || effectiveState === "InProgress";
  const idsMatch = !!currentUser?.id && !!responsiblePhysician?.id && currentUser.id === responsiblePhysician.id;
  const namesMatch = !!currentUser?.name && !!responsiblePhysician?.name && 
    currentUser.name.trim().toLowerCase() === responsiblePhysician.name.trim().toLowerCase();
  
  // Debug logging
  console.log("[PatientSummary] Permission check:", {
    effectiveState,
    stateAllowsEditing,
    currentUserId: currentUser?.id,
    responsiblePhysicianId: responsiblePhysician?.id,
    responsiblePhysicianName: responsiblePhysician?.name,
    currentUserName: currentUser?.name,
    idsMatch,
    namesMatch,
  });
  
  const canEdit = stateAllowsEditing && (idsMatch || (namesMatch && !responsiblePhysician?.id));

  // Mutations for creating/updating patient summary
  const updateSummaryMutation = useUpdatePatientData();

  const handleSave = useCallback(async () => {
    if (!editingText.trim()) {
        return;
    }

    setIsUpdating(true);

    try {
      // The new endpoint handles both create and update (upsert)
      await updateSummaryMutation.mutateAsync({
        handoverId,
        summaryText: editingText,
        illnessSeverity: patientDataProp?.illnessSeverity || "stable", // Default to stable if not present
      });

      setIsEditing(false);
      setEditingText("");
      
      // Notify parent component that save was successful
      if (onSaveSuccess) {
        onSaveSuccess();
      }

    } catch (error) {
      console.error("Failed to save patient summary:", error);
      // Handle error - could show a toast notification here
    } finally {
      setIsUpdating(false);
    }
  }, [
    handoverId,
    updateSummaryMutation,
    editingText,
    patientDataProp,
    onSaveSuccess
  ]);

  useImperativeHandle(ref, () => ({
    save: handleSave
  }));

  // Auto-start editing when in fullscreen with autoEdit
  useEffect(() => {
    if (fullscreenMode && autoEdit && canEdit && !isEditing) {
      setIsEditing(true);
      setEditingText(displayText);
    }
  }, [fullscreenMode, autoEdit, canEdit, displayText, isEditing]);


  const getTimeAgo = (): string => {
    const updatedAt = patientDataProp?.updatedAt;
    if (!updatedAt) return t("time.never");

    try {
      const now = new Date();
      const lastUpdated = new Date(updatedAt);
      
      // Check if date is valid
      if (isNaN(lastUpdated.getTime())) {
        console.warn("[PatientSummary] Invalid updatedAt date:", updatedAt);
        return t("time.never");
      }

      const diffMs = now.getTime() - lastUpdated.getTime();
      const diffMinutes = Math.floor(diffMs / (1000 * 60));
      const diffHours = Math.floor(diffMs / (1000 * 60 * 60));
      const diffDays = Math.floor(diffHours / 24);

      // Show minutes if less than 1 hour
      if (diffMinutes < 1) return t("time.justNow", "Hace un momento");
      if (diffMinutes < 60) return t("time.minutesAgo", "Hace {{count}} minutos", { count: diffMinutes });
      
      // Show hours if less than 24 hours
      if (diffHours < 24) return t("time.hoursAgo", { count: diffHours });
      
      // Show days
      return t("time.daysAgo", { count: diffDays });
    } catch (error) {
      console.error("[PatientSummary] Error calculating time ago:", error, updatedAt);
      return t("time.never");
    }
  };

  // Handle click for editing or fullscreen - SIMPLIFIED FOR SINGLE CLICK
  const handleClick = (): void => {
    if (!canEdit) return;

    if (fullscreenMode) {
      // If in fullscreen, just start editing
      setIsEditing(true);
      setEditingText(displayText);
    } else if (onRequestFullscreen) {
      // If not in fullscreen, go to fullscreen with auto-edit
      onRequestFullscreen();
    } else {
      // Fallback to regular editing
      setIsEditing(true);
      setEditingText(displayText);
    }
  };

  // In fullscreen mode, optimize for writing
  const contentHeight = fullscreenMode ? "min-h-[60vh]" : "h-80";


  return (
    <div className="space-y-4">
      {isEditing && canEdit ? (
        /* Editing Mode - Optimized border radius */
        <div className="space-y-4">
          <div className="relative">
            <div
              className={`relative bg-white border border-gray-300 ${fullscreenMode ? "rounded-lg" : "rounded-t-none rounded-b-lg"} shadow-sm`}
            >
              {/* Header with subtle gray background and top rounded corners */}
              <div
                className={`flex items-center justify-between px-6 py-4 border-b border-gray-200 bg-gray-50 ${fullscreenMode ? "rounded-t-lg" : "rounded-t-lg"}`}
              >
                <div className="flex items-center space-x-3">
                  <div className="w-7 h-7 bg-gray-100 rounded-md flex items-center justify-center">
                    <Edit className="w-4 h-4 text-gray-600" />
                  </div>
                  <h4 className="text-lg font-medium text-gray-800">
                    {fullscreenMode
                      ? t("editing.fullscreenTitle")
                      : t("editing.title")}
                  </h4>
                </div>
                <div className="flex items-center space-x-2">
                  {isUpdating && (
                    <div className="flex items-center space-x-2 text-sm text-gray-600">
                      <div className="w-4 h-4 border-2 border-gray-400 border-t-gray-600 rounded-full animate-spin"></div>
                      <span>{t("editing.updating")}</span>
                    </div>
                  )}
                </div>
              </div>

              {/* Content area - optimized height for fullscreen */}
              <div className={`relative ${contentHeight}`}>
                <ScrollArea className="h-full">
                  <Textarea
                    className={`w-full h-full ${fullscreenMode ? "min-h-[60vh]" : "min-h-[320px]"} border-0 bg-transparent p-4 resize-none text-gray-900 leading-relaxed placeholder:text-gray-400 focus:outline-none focus:ring-0 focus:ring-offset-0 focus-visible:ring-0 focus-visible:ring-offset-0 rounded-none`}
                    placeholder={t("editing.placeholder")}
                    value={currentText}
                    style={{
                      fontFamily: "system-ui, -apple-system, sans-serif",
                      fontSize: fullscreenMode ? "16px" : "14px",
                      lineHeight: "1.6",
                      background: "transparent !important",
                    }}
                    onChange={((event: React.ChangeEvent<HTMLTextAreaElement>) => {
                      setEditingText(event.target.value);
                      if (onContentChange) {
                        onContentChange();
                      }
                    }) as React.ChangeEventHandler<HTMLTextAreaElement>}
                  />
                </ScrollArea>
              </div>

              {/* Footer with save controls - ONLY SHOW IF NOT HIDING CONTROLS */}
              {!hideControls && (
                <div
                  className={`flex items-center justify-between px-4 py-2 border-t border-gray-200 bg-gray-50 ${fullscreenMode ? "rounded-b-lg" : "rounded-b-lg"}`}
                >
                  <div className="flex items-center space-x-3 text-xs text-gray-500">
                    <span>
                      {currentText.split(" ").length} {t("editing.words")}
                    </span>
                    <span>
                      {currentText.split("\n").length} {t("editing.lines")}
                    </span>
                  </div>
                  <div className="flex space-x-2">
                    <Button
                      className="text-xs text-gray-600 hover:bg-gray-100 h-7 px-2"
                      disabled={isUpdating}
                      size="sm"
                      variant="ghost"
                      onClick={() => { setIsEditing(false); }}
                    >
                      {t("editing.cancel")}
                    </Button>
                    <Button
                      className="text-xs bg-blue-600 hover:bg-blue-700 text-white h-7 px-3"
                      disabled={isUpdating || !currentText.trim()}
                      size="sm"
                      onClick={() => { void handleSave(); }}
                    >
                      <Save className="w-3 h-3 mr-1" />
                      {t("editing.save")}
                    </Button>
                  </div>
                </div>
              )}

              {/* Word count footer when controls are hidden */}
              {hideControls && (
                <div
                  className={`flex items-center justify-between px-4 py-2 border-t border-gray-200 bg-gray-50 ${fullscreenMode ? "rounded-b-lg" : "rounded-b-lg"}`}
                >
                  <div className="flex items-center space-x-3 text-xs text-gray-500">
                    <span>
                      {currentText.split(" ").length} {t("editing.words")}
                    </span>
                    <span>
                      {currentText.split("\n").length} {t("editing.lines")}
                    </span>
                  </div>
                  <div className="text-xs text-gray-500">
                    {fullscreenMode
                      ? t("editing.useSaveButtonAbove")
                      : t("editing.useFullscreenControls")}
                  </div>
                </div>
              )}
            </div>
          </div>
        </div>
      ) : (
        /* View Mode - Optimized border radius and click handling */
        <div className="relative group">
          <div
            role={canEdit ? "button" : undefined}
            tabIndex={canEdit ? 0 : undefined}
            aria-label={
              canEdit ? t("view.editAriaLabel") : undefined
            }
            className={`bg-white border border-gray-200 ${fullscreenMode ? "rounded-lg" : "rounded-t-none rounded-b-lg"} shadow-sm hover:shadow-md hover:border-gray-300 transition-all duration-200 ${
              canEdit ? "cursor-pointer" : ""
            }`}
            onClick={handleClick}
            onKeyDown={((event: React.KeyboardEvent<HTMLDivElement>) => {
              if (
                (event.key === "Enter" || event.key === " ") &&
                canEdit) {
                event.preventDefault();
                handleClick();
              }
            }) as React.KeyboardEventHandler<HTMLDivElement>}
          >
            {/* Header with subtle gray background and top rounded corners */}
            <div
              className={`flex items-center justify-between px-6 py-4 border-b border-gray-200 bg-gray-50/50 ${fullscreenMode ? "rounded-t-lg" : "rounded-t-lg"}`}
            >
              <div className="flex items-center space-x-3">
                <div className="w-7 h-7 bg-gray-100 rounded-md flex items-center justify-center">
                  <FileText className="w-4 h-4 text-gray-600" />
                </div>
                <div className="flex items-center space-x-3">
                  <h4 className="text-lg font-medium text-gray-800">
                    {fullscreenMode
                      ? t("view.fullscreenTitle")
                      : t("view.title")}
                  </h4>
                  <Badge
                    className="text-xs bg-orange-50 text-orange-700 border-orange-200 hover:bg-orange-100 transition-colors"
                    variant="outline"
                  >
                    <Shield className="w-3 h-3 mr-1" />
                    {t("responsiblePhysician")}: {responsiblePhysician?.name || t("unassigned")}
                  </Badge>
                </div>
              </div>
              <div className="flex items-center space-x-2">
                <div className="w-2 h-2 bg-gray-400 rounded-full"></div>
                <span className="text-sm text-gray-500 font-medium">
                  {t("view.current")}
                </span>
              </div>
            </div>

            {/* Content area - optimized height for fullscreen */}
            <div className={`relative ${contentHeight}`}>
              <ScrollArea className="h-full">
                <div
                  className="text-gray-800 leading-relaxed whitespace-pre-line p-4"
                  style={{
                    fontFamily: "system-ui, -apple-system, sans-serif",
                    fontSize: fullscreenMode ? "16px" : "14px",
                    lineHeight: "1.6",
                  }}
                >
                  {displayText || (
                    <span className="text-gray-500 italic">
                      {t("view.noSummaryMessage")}
                    </span>
                  )}
                </div>
              </ScrollArea>
            </div>

            {/* Footer with subtle gray background and bottom rounded corners */}
            <div
              className={`flex items-center justify-between px-4 py-2 border-t border-gray-200 bg-gray-50/50 ${fullscreenMode ? "rounded-b-lg" : "rounded-b-lg"}`}
            >
              <div className="flex items-center space-x-3 text-xs text-gray-500">
                <div className="flex items-center space-x-1">
                  <Clock className="w-3 h-3" />
                  <span>
                    {t("view.updated")} {getTimeAgo()}
                  </span>
                </div>
                <span>•</span>
                <span>
                  {displayText.split(" ").length} {t("view.words")}
                </span>
              </div>
              {canEdit && (
                <div className="opacity-0 group-hover:opacity-100 transition-opacity">
                  <div className="flex items-center space-x-1 text-xs text-gray-600">
                    <Edit className="w-3 h-3" />
                    <span>{t("view.clickToEdit")}</span>
                  </div>
                </div>
              )}
            </div>
          </div>

          {/* Permission notice for non-authorized users */}
          {!canEdit && (
            <div className="mt-3 p-3 bg-orange-50 border border-orange-200 rounded-lg">
              <div className="flex flex-col space-y-1 text-sm text-orange-700">
                <div className="flex items-center space-x-2">
                  <Lock className="w-4 h-4" />
                  <span>
                    {t("permission.only")} {responsiblePhysician?.name || t("unassigned")}{" "}
                    {t("permission.canModify")}
                  </span>
                </div>
              </div>
            </div>
          )}
        </div>
      )}
    </div>
  );
});

PatientSummary.displayName = "PatientSummary";
