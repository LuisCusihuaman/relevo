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
import { useCurrentUser } from "@/hooks/useCurrentUser";
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
import { useHandover } from "@/api/endpoints/handovers";
import { useStartHandover, useReadyHandover, useAcceptHandover, useCompleteHandover, useCancelHandover, useRejectHandover } from "@/api";
import { Footer } from "../components/handover/layout/Footer";
import { Header } from "../components/handover/layout/Header";
import { MainContent } from "../components/handover/layout/MainContent";
import { useRouteContext } from "@tanstack/react-router";

interface HandoverProps {
  onBack?: () => void;
}

export default function HandoverPage({ onBack }: HandoverProps = {}): JSX.Element {
  // Get route parameters
  const { handoverId: routeHandoverId } = useRouteContext({ from: "/_authenticated/$patientSlug/$handoverId" });

  // Core state
  const [handoverComplete, setHandoverComplete] = useState(false);

  // UI state
  const [showHistory, setShowHistory] = useState(false);
  const [showComments, setShowComments] = useState(false);
  const [focusMode, setFocusMode] = useState(false);
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
  const { user: currentUser, isLoading: userLoading } = useCurrentUser();
  const { data: handoverData, isLoading: handoverLoading } = useHandover(routeHandoverId);
  const { patientData, isLoading: patientLoading } = usePatientHandoverData(handoverData);
  const { t } = useTranslation("handover");

  // State transition mutations
  const { mutate: readyState } = useReadyHandover();
  const { mutate: startState } = useStartHandover();
  const { mutate: acceptState } = useAcceptHandover();
  const { mutate: completeState } = useCompleteHandover();
  const { mutate: cancelState } = useCancelHandover();
  const { mutate: rejectState } = useRejectHandover();

  const handoverId = handoverData?.id || routeHandoverId;

  const handleReady = (): void => { handoverId && readyState(handoverId); };
  const handleStart = (): void => { handoverId && startState(handoverId); };
  const handleAccept = (): void => { handoverId && acceptState({ handoverId }); };
  const handleComplete = (): void => { handoverId && completeState({ handoverId }); };
  const handleCancel = (): void => { handoverId && cancelState(handoverId); };
  const handleReject = (): void => { handoverId && rejectState({ handoverId, reason: "No reason provided" }); };

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

  // Handle escape key to exit focus mode or fullscreen editing
  useEffect((): (() => void) => {
    const handleKeyDown = (event: KeyboardEvent): void => {
      if (event.key === "Escape") {
        if (fullscreenEditing) {
          setFullscreenEditing(null);
        } else if (focusMode) {
          setFocusMode(false);
        }
      }
    };

    document.addEventListener("keydown", handleKeyDown);
    return (): void => { document.removeEventListener("keydown", handleKeyDown); };
  }, [focusMode, fullscreenEditing]);

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

  // Error state - if handover not found
  if (!handoverData && !handoverLoading) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <div className="text-red-500 mb-4">
            <svg className="h-12 w-12 mx-auto" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L3.732 16.5c-.77.833.192 2.5 1.732 2.5z" />
            </svg>
          </div>
          <h3 className="text-lg font-semibold text-gray-900 mb-2">Handover Not Found</h3>
          <p className="text-gray-600">The requested handover could not be found.</p>
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
            currentUser={currentUser}
          />
        )}

        {/* Desktop History Sidebar - Left side */}
        {showHistory && !focusMode && !isMobile && !fullscreenEditing && (
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
            focusMode={focusMode}
            getSessionDuration={getSessionDuration}
            getSyncStatusDisplay={getSyncStatusDisplay}
            getTimeUntilHandover={getTimeUntilHandover}
            setFocusMode={setFocusMode}
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
                focusMode={focusMode}
                getSessionDuration={getSessionDuration}
                handleOpenDiscussion={handleOpenDiscussion}
                handleOpenFullscreenEdit={handleOpenFullscreenEdit}
                layoutMode={layoutMode}
                setHandoverComplete={setHandoverComplete}
                setSyncStatus={handleSyncStatusChange}
                syncStatus={syncStatus}
                handoverData={handoverData}
              />
            </div>
          </div>

          {/* Footer */}
          <Footer
            focusMode={focusMode}
            fullscreenEditing={!!fullscreenEditing}
            getSessionDuration={getSessionDuration}
            getTimeUntilHandover={getTimeUntilHandover}
            handoverComplete={handoverComplete}
            handoverState={handoverData?.stateName}
            patientData={patientData}
            onAccept={handleAccept}
            onCancel={handleCancel}
            onComplete={handleComplete}
            onReady={handleReady}
            onReject={handleReject}
            onStart={handleStart}
          />
        </SidebarInset>

        {/* Desktop Collaboration Sidebar - Right side */}
        {showComments && !focusMode && !isMobile && !fullscreenEditing && (
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
              focusMode={focusMode}
              fullscreenEditing={!!fullscreenEditing}
              getSessionDuration={getSessionDuration}
              getTimeUntilHandover={getTimeUntilHandover}
              handoverId={handoverData?.id}
              handleNavigateToSection={handleNavigateToSection}
              participants={[]}
              patientData={patientData}
              setFocusMode={setFocusMode}
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
