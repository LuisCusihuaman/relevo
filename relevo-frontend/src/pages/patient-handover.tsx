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
import { type JSX, useEffect } from "react";
import { useTranslation } from "react-i18next";
import { X, FileText, ArrowLeft } from "lucide-react";
import { useParams, useNavigate } from "@tanstack/react-router";
import {
	CollaborationPanel,
	FullscreenEditor,
	HandoverHistory,
	MobileMenus,
} from "@/components/handover";
import { Header } from "@/components/handover/layout/Header";
import { MainContent } from "@/components/handover/layout/MainContent";
import { useHandoverUIStore } from "@/store/handover-ui.store";
import { usePatientCurrentHandover } from "@/hooks/usePatientCurrentHandover";
import { useHandover } from "@/api/endpoints/handovers";
import { usePatientHandoverData } from "@/hooks/usePatientHandoverData";
import type { PatientHandoverData, HandoverDetail as Handover } from "@/types/domain";
import type { UserInfo } from "@/hooks/useCurrentPhysician";

/**
 * PatientHandoverPage - Main page for viewing/editing the active handover of a patient
 * 
 * Rule: Concise-FP - Functional component
 * 
 * This page:
 * 1. Gets patientId from URL params
 * 2. Resolves the active handover using usePatientCurrentHandover
 * 3. Shows the handover if found, or NoActiveHandoverUI if not
 */

// Context type for components that need handover data
export type PatientHandoverContextData = {
	handoverId: string;
	patientId: string;
	handoverData: Handover | null;
	patientData: PatientHandoverData | null;
	currentUser: UserInfo;
	assignedPhysician: { id?: string; name: string; initials: string; role: string };
	receivingPhysician: { id?: string; name: string; initials: string; role: string };
	isLoading: boolean;
	error: Error | null;
};

function NoActiveHandoverUI({ patientId }: { patientId: string }): JSX.Element {
	const { t } = useTranslation("handover");
	const navigate = useNavigate();

	return (
		<div className="min-h-screen bg-gray-50 flex items-center justify-center">
			<div className="text-center max-w-md px-4">
				<div className="mb-6">
					<div className="mx-auto w-16 h-16 bg-gray-100 rounded-full flex items-center justify-center">
						<FileText className="w-8 h-8 text-gray-400" />
					</div>
				</div>
				<h3 className="text-lg font-semibold text-gray-900 mb-2">
					{t("noActiveHandover", "No hay handover activo")}
				</h3>
				<p className="text-gray-600 mb-6">
					{t("noActiveHandoverDescription", "Este paciente no tiene un pase de guardia en curso. Puede revisar el historial de traspasos anteriores.")}
				</p>
				<div className="flex flex-col sm:flex-row gap-3 justify-center">
					<Button
						variant="outline"
						onClick={() => { void navigate({ to: "/patients" }); }}
					>
						<ArrowLeft className="w-4 h-4 mr-2" />
						{t("backToPatients", "Volver a pacientes")}
					</Button>
				</div>
				<p className="mt-4 text-xs text-gray-500">
					Patient ID: {patientId}
				</p>
			</div>
		</div>
	);
}

function LoadingUI(): JSX.Element {
	return (
		<div className="min-h-screen bg-gray-50 flex items-center justify-center">
			<div className="text-center">
				<div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto mb-4"></div>
				<p className="text-gray-600">Cargando datos del paciente...</p>
			</div>
		</div>
	);
}

function ErrorUI({ error, onBack }: { error: Error; onBack?: () => void }): JSX.Element {
	return (
		<div className="min-h-screen bg-gray-50 flex items-center justify-center">
			<div className="text-center">
				<div className="text-red-500 mb-4">
					<svg className="h-12 w-12 mx-auto" fill="none" stroke="currentColor" viewBox="0 0 24 24">
						<path d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L3.732 16.5c-.77.833.192 2.5 1.732 2.5z" strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} />
					</svg>
				</div>
				<h3 className="text-lg font-semibold text-gray-900 mb-2">
					Error al cargar
				</h3>
				<p className="text-gray-600">
					{error.message}
				</p>
				{onBack && (
					<button
						className="mt-4 px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
						onClick={onBack}
					>
						Volver
					</button>
				)}
			</div>
		</div>
	);
}

// Inner component that renders once we have a handoverId
function HandoverContent({ 
	handoverId, 
	patientId,
	onBack 
}: { 
	handoverId: string; 
	patientId: string;
	onBack?: () => void;
}): JSX.Element {
	const { t } = useTranslation("handover");
	const isMobile = useIsMobile();

	// Store
	const showHistory = useHandoverUIStore(state => state.showHistory);
	const setShowHistory = useHandoverUIStore(state => state.setShowHistory);
	const showComments = useHandoverUIStore(state => state.showComments);
	const setShowComments = useHandoverUIStore(state => state.setShowComments);
	const fullscreenEditing = useHandoverUIStore(state => state.fullscreenEditing);
	const reset = useHandoverUIStore(state => state.reset);

	// Fetch handover data
	const {
		data: handoverData,
		isLoading: handoverLoading,
		error: handoverError,
	} = useHandover(handoverId);
	
	const {
		patientData,
		isLoading: patientLoading,
		error: patientError,
	} = usePatientHandoverData(handoverId);

	// Reset store on unmount
	useEffect(() => {
		return (): void => { reset(); };
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

	const isLoading = handoverLoading || patientLoading;
	const error = (handoverError as Error) || (patientError as Error) || null;

	if (isLoading) {
		return <LoadingUI />;
	}

	if (error) {
		return <ErrorUI error={error} onBack={onBack} />;
	}

	if (!handoverData) {
		return <NoActiveHandoverUI patientId={patientId} />;
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
									onClick={() => { setShowHistory(false); }}
								>
									<X className="h-4 w-4" />
								</Button>
							</div>
						</SidebarHeader>
						<SidebarContent>
							<HandoverHistory
								hideHeader
								patientId={patientId}
								currentHandoverId={handoverId}
								patientData={mappedPatientData}
								onClose={() => { setShowHistory(false); }}
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
									onClick={() => { setShowComments(false); }}
								>
									<X className="h-4 w-4" />
								</Button>
							</div>
						</SidebarHeader>
						<SidebarContent>
							<CollaborationPanel
								hideHeader
								handoverId={handoverId}
								onClose={() => { setShowComments(false); }}
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

export default function PatientHandoverPage(): JSX.Element {
	const { patientId } = useParams({ from: "/_authenticated/patient/$patientId" });
	const navigate = useNavigate();

	// Resolve active handover for this patient
	const { 
		currentHandover, 
		hasActiveHandover, 
		isLoading, 
		error 
	} = usePatientCurrentHandover(patientId);

	const handleBack = (): void => {
		void navigate({ to: "/patients" });
	};

	// Show loading while resolving handover
	if (isLoading) {
		return <LoadingUI />;
	}

	// Show error if failed to load timeline
	if (error) {
		return <ErrorUI error={error} onBack={handleBack} />;
	}

	// No active handover for this patient
	if (!hasActiveHandover || !currentHandover) {
		return <NoActiveHandoverUI patientId={patientId} />;
	}

	// Render the handover content
	return (
		<HandoverContent 
			handoverId={currentHandover.id} 
			patientId={patientId}
			onBack={handleBack}
		/>
	);
}
