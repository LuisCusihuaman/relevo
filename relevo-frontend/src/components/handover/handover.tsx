import { currentUser, patientData } from "@/common/constants";
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
import { useCallback, useEffect, useRef, useState } from "react";
import { useTranslation } from "react-i18next";
import { X } from "lucide-react";
import {
  CollaborationPanel,
  FullscreenEditor,
  HandoverHistory,
  MobileMenus,
  useHandoverSession,
} from ".";
import { Footer } from "./layout/Footer";
import { Header } from "./layout/Header";
import { MainContent } from "./layout/MainContent";

interface HandoverProps {
  onBack?: () => void;
}

export default function App({ onBack }: HandoverProps = {}): JSX.Element {
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
  const { t } = useTranslation("handover");

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
            handleNavigateToSection={handleNavigateToSection}
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
