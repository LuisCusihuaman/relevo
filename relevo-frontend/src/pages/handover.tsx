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
import { usePatientHandoverData } from "@/hooks/usePatientHandoverData";
import { type JSX, useEffect } from "react";
import { useTranslation } from "react-i18next";
import { X } from "lucide-react";
import {
  CollaborationPanel,
  FullscreenEditor,
  HandoverHistory,
  MobileMenus,
} from "../components/handover";
import { useHandover } from "@/api/endpoints/handovers";
import { Header } from "../components/handover/layout/Header";
import { MainContent } from "../components/handover/layout/MainContent";
import { useParams } from "@tanstack/react-router";
import { useHandoverUIStore } from "@/store/handover-ui.store";

interface HandoverProps {
  onBack?: () => void;
}

export default function HandoverPage({ onBack }: HandoverProps = {}): JSX.Element {
  // Get route parameters
  const { handoverId } = useParams({ from: "/_authenticated/$patientSlug/$handoverId" }) as unknown as { handoverId: string };

  // Store
  const {
    showHistory,
    setShowHistory,
    showComments,
    setShowComments,
    fullscreenEditing,
    reset
  } = useHandoverUIStore();

  // Custom hooks
  const isMobile = useIsMobile();
  
  // Data fetching (mainly for loading/error states of the page)
  const { data: handoverData, isLoading: handoverLoading, error: handoverError } = useHandover(handoverId);
  const { patientData, isLoading: patientLoading } = usePatientHandoverData(handoverId);

  const { t } = useTranslation("handover");

  // Reset store on unmount
  useEffect(() => {
    return () => reset();
  }, [reset]);

  const mappedPatientData = patientData ? {
    id: patientData.id || "",
    name: patientData.name || "",
    dob: patientData.dob || "",
    mrn: patientData.mrn || "",
    admissionDate: patientData.admissionDate || "",
    currentDateTime: new Date().toISOString(),
    primaryTeam: patientData.primaryTeam || "",
    primaryDiagnosis: patientData.primaryDiagnosis || "",
    room: patientData.room || "",
    unit: patientData.unit || "",
    assignedPhysician: patientData.assignedPhysician || null,
    receivingPhysician: patientData.receivingPhysician || null,
  } : {
    id: "",
    name: "",
    dob: "",
    mrn: "",
    admissionDate: "",
    currentDateTime: "",
    primaryTeam: "",
    primaryDiagnosis: "",
    room: "",
    unit: "",
    assignedPhysician: null,
    receivingPhysician: null,
  };

  // Loading state
  if (patientLoading || handoverLoading) {
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
            <svg className="h-12 w-12 mx-auto" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L3.732 16.5c-.77.833.192 2.5 1.732 2.5z" strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} />
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
          {import.meta.env.DEV && handoverError && (
            <details className="mt-4 text-left">
              <summary className="cursor-pointer text-sm text-gray-500">Technical Details</summary>
              <pre className="mt-2 text-xs bg-gray-100 p-2 rounded overflow-auto max-w-md">
                {JSON.stringify(handoverError, null, 2)}
              </pre>
            </details>
          )}
          {onBack && (
            <button
              className="mt-4 px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
              onClick={onBack}
            >
              Go Back
            </button>
          )}
        </div>
      </div>
    );
  }

  return (
    <TooltipProvider>
      <SidebarProvider>
        {/* Fullscreen Editor */}
        {fullscreenEditing && (
          <FullscreenEditor />
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
                handoverId={handoverData?.id || ""}
                patientData={mappedPatientData}
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
          <Header onBack={onBack} />

          {/* Main Content */}
          <div className="px-4 sm:px-6 lg:px-8 py-6 sm:py-8 pb-24">
            <div className="max-w-7xl mx-auto">
              <MainContent />
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
                onClose={() => {
                  setShowComments(false);
                }}
              />
            </SidebarContent>
          </Sidebar>
        )}

        {/* Mobile Menus */}
        {isMobile && (
            <MobileMenus />
        )}
      </SidebarProvider>
    </TooltipProvider>
  );
}
