import type { ExpandedSections, FullscreenEditingState, SyncStatus } from "@/common/types";
import { Button } from "@/components/ui/button";
import {
  Sidebar,
  SidebarContent,
  SidebarHeader,
  SidebarInset,
  SidebarProvider,
} from "@/components/ui/sidebar";
import { TooltipProvider } from "@/components/ui/tooltip";
import { useIsMobile } from "@/hooks/use-mobile";
import { useSyncStatus } from "@/components/handover/hooks/useSyncStatus";
import { useUser } from "@clerk/clerk-react";
import { usePatientHandoverData } from "@/hooks/usePatientHandoverData";
import { type JSX, useCallback, useEffect, useRef, useState } from "react";
import { useTranslation } from "react-i18next";
import { X } from "lucide-react";
import {
  CollaborationPanel,
  FullscreenEditor,
  HandoverHistory,
  MobileMenus,
  useHandoverSession,
} from "../components/handover";
import { useHandover, useStartHandover, useReadyHandover, useAcceptHandover, useCompleteHandover, useCancelHandover, useRejectHandover } from "@/api/endpoints/handovers";
import { Header } from "../components/handover/layout/Header";
import { MainContent } from "../components/handover/layout/MainContent";
import { useParams } from "@tanstack/react-router";

interface HandoverProps {
  onBack?: () => void;
}

export default function HandoverPage({ onBack }: HandoverProps = {}): JSX.Element {
  // Get route parameters
  const { handoverId } = useParams({ from: "/_authenticated/$patientSlug/$handoverId" });

  // Core state
  const [handoverComplete, setHandoverComplete] = useState(false);

  // UI state
  const [showHistory, setShowHistory] = useState(false);
  const [showComments, setShowComments] = useState(false);
  const [showCollaborators, setShowCollaborators] = useState(false);
  const [showMobileMenu, setShowMobileMenu] = useState(false);

  // Fullscreen editing state
  const [fullscreenEditing, setFullscreenEditing] =
    useState<FullscreenEditingState | null>(null);
  const saveFunctionRef = useRef<(() => void) | null>(null);

  // Layout and content state
  const [layoutMode] = useState<"single" | "columns">("columns");
  const [expandedSections, setExpandedSections] = useState<ExpandedSections>({
    illness: true, // Start with first section open
    patient: false,
    actions: false,
    awareness: false,
    synthesis: false,
  });

  // Custom hooks
  const isMobile = useIsMobile();
  const { getTimeUntilHandover, getSessionDuration } = useHandoverSession();
  const { syncStatus, setSyncStatus, getSyncStatusDisplay } = useSyncStatus();
  const { user: clerkUser } = useUser();

  // Simplified user object from Clerk user for components that need basic user info
  const currentUser = clerkUser ? {
    name: clerkUser.fullName || `${clerkUser.firstName || ''} ${clerkUser.lastName || ''}`.trim() || 'Unknown User',
    initials: (clerkUser.fullName || `${clerkUser.firstName || ''} ${clerkUser.lastName || ''}`)
      ?.split(" ")
      .map((n) => n[0])
      .join("")
      .toUpperCase() || 'U',
    role: 'Doctor' // All users are Doctors for now
  } : null;

  const userLoading = false; // Clerk user is available synchronously
  const { data: handoverData, isLoading: handoverLoading, error: handoverError } = useHandover(handoverId);
  const { patientData, isLoading: patientLoading } = usePatientHandoverData(handoverId);

  const { t } = useTranslation("handover");

  // State transition mutations
  const { mutate: readyState } = useReadyHandover();
  const { mutate: startState } = useStartHandover();
  const { mutate: acceptState } = useAcceptHandover();
  const { mutate: completeState } = useCompleteHandover();
  const { mutate: cancelState } = useCancelHandover();
  const { mutate: rejectState } = useRejectHandover();

  const currentHandoverId = handoverData?.id || handoverId;

  const handleReady = (): void => { currentHandoverId && readyState(currentHandoverId); };
  const handleStart = (): void => { currentHandoverId && startState(currentHandoverId); };
  const handleAccept = (): void => { currentHandoverId && acceptState(currentHandoverId); };
  const handleComplete = (): void => { currentHandoverId && completeState(currentHandoverId); };
  const handleCancel = (): void => { currentHandoverId && cancelState(currentHandoverId); };
  const handleReject = (): void => { currentHandoverId && rejectState({ handoverId: currentHandoverId, reason: "No reason provided" }); };

  // Event handlers
  const handleSyncStatusChange = (status: SyncStatus): void => {
    setSyncStatus(status);
  };

  const handleNavigateToSection = (section: string): void => {
    if (layoutMode === "single") {
      setExpandedSections((previous) => ({ ...previous, [section]: true }));
    }
    console.log(`Navigating to I-PASS section: ${section}`);
  };

  const handleOpenDiscussion = (): void => {
    setShowComments(true);
  };

  const handleOpenFullscreenEdit = (
    component: "patient-summary" | "situation-awareness",
    autoEdit: boolean = true,
  ): void => {
    setFullscreenEditing({ component, autoEdit });
  };

  const handleCloseFullscreenEdit = (): void => {
    setFullscreenEditing(null);
  };

  const handleFullscreenSave = useCallback((): void => {
    if (saveFunctionRef.current) {
      saveFunctionRef.current();
    }
  }, []);

  const handleSaveReady = useCallback((saveFunction: () => void): void => {
    saveFunctionRef.current = saveFunction;
  }, []);

  // Handle escape key to exit fullscreen editing
  useEffect((): (() => void) => {
    const handleKeyDown = (event: KeyboardEvent): void => {
      if (event.key === "Escape") {
        if (fullscreenEditing) {
          setFullscreenEditing(null);
        }
      }
    };

    document.addEventListener("keydown", handleKeyDown);
    return (): void => { document.removeEventListener("keydown", handleKeyDown); };
  }, [fullscreenEditing]);

  // Loading state
  if (userLoading || patientLoading || handoverLoading) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto mb-4"></div>
          <p className="text-gray-600">Loading handover data...</p>
        </div>
      </div>
    );
  }

  // Error state - if handover not found or API error
  if ((!handoverData && !handoverLoading) || handoverError) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <div className="text-red-500 mb-4">
            <svg className="h-12 w-12 mx-auto" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L3.732 16.5c-.77.833.192 2.5 1.732 2.5z" />
            </svg>
          </div>
          <h3 className="text-lg font-semibold text-gray-900 mb-2">
            {handoverError ? "Error Loading Handover" : "Handover Not Found"}
          </h3>
          <p className="text-gray-600">
            {handoverError
              ? `Failed to load handover data: ${handoverError.message}`
              : "The requested handover could not be found."
            }
          </p>
          {process.env.NODE_ENV === "development" && handoverError && (
            <details className="mt-4 text-left">
              <summary className="cursor-pointer text-sm text-gray-500">Technical Details</summary>
              <pre className="mt-2 text-xs bg-gray-100 p-2 rounded overflow-auto max-w-md">
                {JSON.stringify(handoverError, null, 2)}
              </pre>
            </details>
          )}
          {onBack && (
            <button
              onClick={onBack}
              className="mt-4 px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
            >
              Go Back
            </button>
          )}
        </div>
      </div>
    );
  }

  // Error state - if no user or patient data available
  if (!currentUser || !patientData) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <div className="text-red-500 mb-4">
            <svg className="h-12 w-12 mx-auto" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L3.732 16.5c-.77.833.192 2.5 1.732 2.5z" />
            </svg>
          </div>
          <h3 className="text-lg font-semibold text-gray-900 mb-2">Unable to Load Data</h3>
          <p className="text-gray-600">Please check your authentication and try again.</p>
        </div>
      </div>
    );
  }

  return (
    <TooltipProvider>
      <SidebarProvider>
        {/* Fullscreen Editor */}
        {fullscreenEditing && (
          <FullscreenEditor
            fullscreenEditing={fullscreenEditing}
            handleCloseFullscreenEdit={handleCloseFullscreenEdit}
            handleFullscreenSave={handleFullscreenSave}
            handleOpenDiscussion={handleOpenDiscussion}
            handleSaveReady={handleSaveReady}
            setSyncStatus={handleSyncStatusChange}
            syncStatus={syncStatus}
            patientData={patientData}
            handoverData={handoverData ? { id: handoverData.id } : undefined}
            currentUser={currentUser ?? undefined}
          />
        )}

        {/* Desktop History Sidebar - Left side */}
        {showHistory && !isMobile && !fullscreenEditing && (
          <Sidebar collapsible="offcanvas" side="left">
            <SidebarHeader className="p-4 border-b border-gray-200">
              <div className="flex items-center justify-between">
                <h2 className="font-semibold text-gray-900">
                  {t("historySidebarTitle")}
                </h2>
                <Button
                  className="h-6 w-6 p-0"
                  size="sm"
                  variant="ghost"
                  onClick={() => {
                    setShowHistory(false);
                  }}
                >
                  <X className="h-4 w-4" />
                </Button>
              </div>
            </SidebarHeader>
            <SidebarContent>
              <HandoverHistory
                hideHeader
                patientData={patientData}
                handoverId={handoverData?.id || ""}
                onClose={() => {
                  setShowHistory(false);
                }}
              />
            </SidebarContent>
          </Sidebar>
        )}

        {/* Main Content Area */}
        <SidebarInset className="min-h-screen bg-gray-50">
          {/* Header */}
          <Header
            getSessionDuration={getSessionDuration}
            getSyncStatusDisplay={getSyncStatusDisplay}
            getTimeUntilHandover={getTimeUntilHandover}
            setShowCollaborators={setShowCollaborators}
            setShowComments={setShowComments}
            setShowHistory={setShowHistory}
            setShowMobileMenu={setShowMobileMenu}
            showCollaborators={showCollaborators}
            showComments={showComments}
            showHistory={showHistory}
            onBack={onBack}
            patientData={patientData}
          />

          {/* Main Content */}
          <div className="px-4 sm:px-6 lg:px-8 py-6 sm:py-8 pb-24">
            <div className="max-w-7xl mx-auto">
              <MainContent
                currentUser={currentUser}
                expandedSections={expandedSections}
                getSessionDuration={getSessionDuration}
                handleOpenDiscussion={handleOpenDiscussion}
                handleOpenFullscreenEdit={handleOpenFullscreenEdit}
                layoutMode={layoutMode}
                setHandoverComplete={setHandoverComplete}
                setSyncStatus={handleSyncStatusChange}
                syncStatus={syncStatus}
                handoverData={handoverData}
                patientData={patientData}
              />
            </div>
          </div>

        </SidebarInset>

        {/* Desktop Collaboration Sidebar - Right side */}
        {showComments && !isMobile && !fullscreenEditing && (
          <Sidebar collapsible="offcanvas" side="right">
            <SidebarHeader className="p-4 border-b border-gray-200">
              <div className="flex items-center justify-between">
                <h2 className="font-semibold text-gray-900">
                  {t("collaborationSidebarTitle")}
                </h2>
                <Button
                  className="h-6 w-6 p-0"
                  size="sm"
                  variant="ghost"
                  onClick={() => {
                    setShowComments(false);
                  }}
                >
                  <X className="h-4 w-4" />
                </Button>
              </div>
            </SidebarHeader>
            <SidebarContent>
              <CollaborationPanel
                hideHeader
                handoverId={handoverData?.id || ""}
                onNavigateToSection={handleNavigateToSection}
                onClose={() => {
                  setShowComments(false);
                }}
              />
            </SidebarContent>
          </Sidebar>
        )}

        {/* Mobile Menus */}
        {isMobile && (
            <MobileMenus
              currentUser={currentUser}
              fullscreenEditing={!!fullscreenEditing}
              getSessionDuration={getSessionDuration}
              getTimeUntilHandover={getTimeUntilHandover}
              handoverId={handoverData?.id}
              handleNavigateToSection={handleNavigateToSection}
              participants={[]}
              patientData={patientData}
              setShowComments={setShowComments}
              setShowHistory={setShowHistory}
              setShowMobileMenu={setShowMobileMenu}
              showComments={showComments}
              showHistory={showHistory}
              showMobileMenu={showMobileMenu}
            />
        )}
      </SidebarProvider>
    </TooltipProvider>
  );
}
