import {
    activeCollaborators,
} from "@/common/constants";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Button } from "@/components/ui/button";
import { Separator } from "@/components/ui/separator";
import {
    Tooltip,
    TooltipContent,
    TooltipTrigger,
} from "@/components/ui/tooltip";
import { useIsMobile } from "@/hooks/use-mobile";
import { Clock, Save, Stethoscope, X } from "lucide-react";
import { useCallback, useEffect, useRef, useState, type JSX } from "react";
import { useTranslation } from "react-i18next";
import { PatientSummary } from "./PatientSummary";
import { SituationAwareness } from "./SituationAwareness";
import { useHandoverUIStore } from "@/store/handover-ui.store";
import { useSyncStatus } from "@/components/handover/hooks/useSyncStatus";
import { usePatientHandoverData } from "@/hooks/usePatientHandoverData";
import { useParams } from "@tanstack/react-router";
import { useUser } from "@clerk/clerk-react";

export function FullscreenEditor(): JSX.Element | null {
  const { t } = useTranslation("fullscreenEditor");
  const isMobile = useIsMobile();
  const saveButtonRef = useRef<HTMLButtonElement>(null);
  
  // Store
  const { 
    fullscreenEditing, 
    setFullscreenEditing, 
    currentSaveFunction,
    setCurrentSaveFunction 
  } = useHandoverUIStore();

  // Hooks
  const { syncStatus, setSyncStatus, getSyncStatusDisplay } = useSyncStatus();
  const { handoverId } = useParams({ from: "/_authenticated/$patientSlug/$handoverId" }) as unknown as { handoverId: string };
  const { patientData } = usePatientHandoverData(handoverId);
  const { user: clerkUser } = useUser();

  const [hasUnsavedChanges, setHasUnsavedChanges] = useState(false);

  // Derived values
  const componentType = fullscreenEditing?.component;

  // Transform user for components
  const currentUser = clerkUser ? {
    id: clerkUser.id,
    name: clerkUser.fullName || `${clerkUser.firstName || ''} ${clerkUser.lastName || ''}`.trim() || 'Unknown User',
    initials: (clerkUser.fullName || `${clerkUser.firstName || ''} ${clerkUser.lastName || ''}`)
      ?.split(" ")
      .map((n) => n[0])
      .join("")
      .toUpperCase() || 'U',
    role: (Array.isArray(clerkUser.publicMetadata['roles']) ? (clerkUser.publicMetadata['roles'] as Array<string>).join(", ") : "Doctor"),
  } : undefined;

  // Get active collaborators with stable reference
  const activeUsers = useRef(
    activeCollaborators
      .filter((user) => user.status === "active" || user.status === "viewing")
      .map((user, index) => ({
        ...user,
        id: `${user.id}-${index}`, // Stable ID
      })),
  ).current;

  // Handle Close
  const handleCloseFullscreenEdit = useCallback(() => {
    setFullscreenEditing(null);
  }, [setFullscreenEditing]);

  // Handle Save (call the function registered in store)
  const handleFullscreenSave = useCallback(() => {
    if (currentSaveFunction) {
        currentSaveFunction();
    }
  }, [currentSaveFunction]);

  // Handle content changes
  const handleContentChange = useCallback((): void => {
    setHasUnsavedChanges(true);
    setSyncStatus("pending");
  }, [setSyncStatus]);

  // Keyboard shortcuts
  useEffect(() => {
    const handleKeyDown = (event: KeyboardEvent): void => {
      if (event.key === "Escape") {
        handleCloseFullscreenEdit();
      } else if ((event.ctrlKey || event.metaKey) && event.key === "s") {
        event.preventDefault();
        handleFullscreenSave();
      }
    };

    document.addEventListener("keydown", handleKeyDown);
    return () => { document.removeEventListener("keydown", handleKeyDown); };
  }, [handleCloseFullscreenEdit, handleFullscreenSave]);

  // Get component title
  const getComponentTitle = (): string => {
    switch (componentType) {
      case "patient-summary":
        return t("titles.patientSummary");
      case "situation-awareness":
        return t("titles.situationAwareness");
      default:
        return t("titles.default");
    }
  };

  const syncDisplay = getSyncStatusDisplay();

  if (!fullscreenEditing) return null;

  return (
    <div className="fixed inset-0 bg-white z-[9999] flex flex-col">
      {/* Header - matches the design exactly */}
      <div className="flex-shrink-0 bg-white border-b border-gray-200 px-4 py-3 sm:px-6 sm:py-4">
        <div className="flex items-center justify-between">
          {/* Left side - RELEVO branding and patient info */}
          <div className="flex items-center space-x-3 min-w-0 flex-1">
            <div className="flex items-center space-x-2 flex-shrink-0">
              <Stethoscope className="w-4 h-4 sm:w-5 sm:h-5 text-gray-900" />
              <h1 className="text-base sm:text-lg font-bold text-gray-900">
                {t("relevo")}
              </h1>
            </div>
            <Separator
              className="h-4 sm:h-6 hidden sm:block"
              orientation="vertical"
            />
            <div className="min-w-0">
              <div className="flex items-center space-x-2">
                <span className="text-sm text-gray-600">
                  {patientData?.name || "Patient"}
                </span>
                <span className="text-sm text-gray-400">•</span>
                <span className="text-sm font-medium text-gray-900">
                  {getComponentTitle()}
                </span>
              </div>
            </div>
          </div>

          {/* Right side - Sync status, collaborators, and actions */}
          <div className="flex items-center space-x-4">
            {/* Sync Status */}
            <div className="flex items-center space-x-2">
              <div
                className={`w-2 h-2 rounded-full ${
                  syncStatus === "synced"
                    ? "bg-green-500"
                    : syncStatus === "pending"
                      ? "bg-yellow-500"
                      : syncStatus === "error"
                        ? "bg-red-500"
                        : "bg-gray-400"
                }`}
              />
              <span className={`text-sm ${syncDisplay.color}`}>
                {syncDisplay.text}
              </span>
            </div>

            {/* Collaborators - shows user avatars like in the image */}
            {activeUsers.length > 0 && (
              <div className="hidden sm:flex items-center space-x-1">
                {activeUsers.slice(0, 3).map((user) => (
                  <Tooltip key={user.id}>
                    <TooltipTrigger asChild>
                      <Avatar className="w-6 h-6 border border-white">
                        <AvatarFallback
                          className={`${user.color} text-white text-xs font-medium`}
                        >
                          {user.initials}
                        </AvatarFallback>
                      </Avatar>
                    </TooltipTrigger>
                    <TooltipContent
                      className="bg-gray-900 text-white text-xs px-2 py-1 border-none shadow-lg"
                      side="bottom"
                    >
                      <div className="text-center">
                        <div className="font-medium">{user.name}</div>
                        <div className="text-gray-300">{user.role}</div>
                      </div>
                    </TooltipContent>
                  </Tooltip>
                ))}
                {activeUsers.length > 3 && (
                  <div className="text-xs text-gray-500 ml-2">
                    {t("moreUsers", { count: activeUsers.length - 3 })}
                  </div>
                )}
              </div>
            )}

            {/* Save Button - Only show for Patient Summary */}
            {fullscreenEditing.component === "patient-summary" && (
              <Button
                ref={saveButtonRef}
                className="bg-gray-900 hover:bg-gray-800 text-white text-xs px-3 h-8"
                disabled={!hasUnsavedChanges || syncStatus === "pending"}
                size="sm"
                onClick={handleFullscreenSave}
              >
                <Save className="w-3 h-3 mr-1" />
                {t("save")}
              </Button>
            )}

            {/* Close button */}
            <Button
              className="h-8 w-8 sm:h-10 sm:w-10 p-0 hover:bg-gray-100 flex-shrink-0"
              size="sm"
              variant="ghost"
              onClick={handleCloseFullscreenEdit}
            >
              <X className="h-4 w-4 sm:h-5 sm:w-5" />
            </Button>
          </div>
        </div>
      </div>

      {/* Content Area - Full available space */}
      <div className="flex-1 min-h-0 overflow-hidden">
        <div className="h-full w-full overflow-auto">
          <div
            className={`w-full h-full ${isMobile ? "p-4" : "max-w-4xl mx-auto p-6"}`}
          >
            {fullscreenEditing.component === "patient-summary" && (
              <div className="h-full">
                <PatientSummary
                  fullscreenMode
                  hideControls
                  autoEdit={fullscreenEditing.autoEdit}
                  currentUser={currentUser}
                  handoverId={handoverId}
                  patientData={patientData || undefined}
                  onContentChange={handleContentChange}
                  onRequestFullscreen={() => {}}
                  onSave={handleFullscreenSave}
                  onSaveReady={setCurrentSaveFunction}
                />
              </div>
            )}

            {fullscreenEditing.component === "situation-awareness" && (
              <div className="h-full">
                <SituationAwareness
                  fullscreenMode
                  hideControls
                  assignedPhysician={patientData?.assignedPhysician ? {
                    name: patientData.assignedPhysician.name,
                    initials: patientData.assignedPhysician.name.split(' ').map(n => n[0]).join('').toUpperCase(),
                    role: patientData.assignedPhysician.role
                  } : { name: "Unknown", initials: "U", role: "Unknown" }}
                  autoEdit={fullscreenEditing.autoEdit}
                  currentUser={currentUser || { name: "Unknown User", initials: "U", role: "Unknown" }}
                  handoverId={handoverId}
                  onContentChange={handleContentChange}
                />
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Footer - Status bar matching the design */}
      <div className="flex-shrink-0 border-t border-gray-200 bg-gray-50 px-4 py-3 sm:px-6">
        <div className="flex items-center justify-between text-sm text-gray-600">
          <div className="flex items-center space-x-4">
            <span>{t("footer.editing")}</span>
            <span>•</span>
            <span>{t("footer.shortcuts")}</span>
          </div>
          <div className="flex items-center space-x-2">
            <Clock className="w-4 h-4" />
            <span>{new Date().toLocaleTimeString()}</span>
          </div>
        </div>
      </div>
    </div>
  );
}
