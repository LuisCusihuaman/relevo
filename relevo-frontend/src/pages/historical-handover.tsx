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
import { type JSX, useEffect, useMemo } from "react";
import { useTranslation } from "react-i18next";
import { X, AlertTriangle, ArrowLeft, Clock } from "lucide-react";
import { useParams, useNavigate } from "@tanstack/react-router";
import {
	CollaborationPanel,
	HandoverHistory,
} from "@/components/handover";
import { Header } from "@/components/handover/layout/Header";
import { MainContent } from "@/components/handover/layout/MainContent";
import { useHandoverUIStore } from "@/store/handover-ui.store";
import { useHandover } from "@/api/endpoints/handovers";
import { usePatientHandoverData } from "@/hooks/usePatientHandoverData";
import dayjs from "dayjs";

/**
 * HistoricalHandoverPage - Read-only view of a completed/cancelled handover
 * 
 * Rule: Concise-FP - Functional component
 * 
 * This page:
 * 1. Gets patientId and handoverId from URL params
 * 2. Shows the handover in read-only mode
 * 3. Displays a prominent banner indicating it's historical
 */

function LoadingUI(): JSX.Element {
	return (
		<div className="min-h-screen bg-gray-50 flex items-center justify-center">
			<div className="text-center">
				<div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto mb-4"></div>
				<p className="text-gray-600">Cargando registro histórico...</p>
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

function NotFoundUI({ handoverId, onBack }: { handoverId: string; onBack?: () => void }): JSX.Element {
	const { t } = useTranslation("handover");

	return (
		<div className="min-h-screen bg-gray-50 flex items-center justify-center">
			<div className="text-center max-w-md px-4">
				<div className="text-gray-400 mb-4">
					<Clock className="h-12 w-12 mx-auto" />
				</div>
				<h3 className="text-lg font-semibold text-gray-900 mb-2">
					{t("handoverNotFound", "Handover no encontrado")}
				</h3>
				<p className="text-gray-600 mb-4">
					{t("handoverNotFoundDescription", "El registro histórico solicitado no existe o fue eliminado.")}
				</p>
				<p className="text-xs text-gray-400 mb-6">
					ID: {handoverId}
				</p>
				{onBack && (
					<Button variant="outline" onClick={onBack}>
						<ArrowLeft className="w-4 h-4 mr-2" />
						{t("back", "Volver")}
					</Button>
				)}
			</div>
		</div>
	);
}

export default function HistoricalHandoverPage(): JSX.Element {
	const { patientId, handoverId } = useParams({ 
		from: "/_authenticated/patient/$patientId/history/$handoverId" 
	});
	const navigate = useNavigate();
	const { t } = useTranslation("handover");
	const isMobile = useIsMobile();

	// Store
	const showHistory = useHandoverUIStore(state => state.showHistory);
	const setShowHistory = useHandoverUIStore(state => state.setShowHistory);
	const showComments = useHandoverUIStore(state => state.showComments);
	const setShowComments = useHandoverUIStore(state => state.setShowComments);
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

	const handleBack = (): void => {
		void navigate({ to: "/patient/$patientId", params: { patientId } });
	};

	const handleBackToPatients = (): void => {
		void navigate({ to: "/patients" });
	};

	// Format completion date
	const completionDateText = useMemo(() => {
		if (!handoverData?.completedAt) return null;
		return dayjs(handoverData.completedAt).format("DD/MM/YYYY HH:mm");
	}, [handoverData?.completedAt]);

	const cancellationDateText = useMemo(() => {
		if (!handoverData?.cancelledAt) return null;
		return dayjs(handoverData.cancelledAt).format("DD/MM/YYYY HH:mm");
	}, [handoverData?.cancelledAt]);

	// Show loading
	if (isLoading) {
		return <LoadingUI />;
	}

	// Show error
	if (error) {
		return <ErrorUI error={error} onBack={handleBackToPatients} />;
	}

	// Handover not found
	if (!handoverData) {
		return <NotFoundUI handoverId={handoverId} onBack={handleBackToPatients} />;
	}

	// Determine status text
	const statusText = handoverData.stateName === "Cancelled" 
		? t("historicalCancelled", "Cancelado")
		: t("historicalCompleted", "Completado");
	
	const dateText = handoverData.stateName === "Cancelled"
		? cancellationDateText
		: completionDateText;

	return (
		<TooltipProvider>
			<SidebarProvider>
				{/* Desktop History Sidebar - Left side */}
				{showHistory && !isMobile && (
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
					{/* Historical Banner */}
					<div className="bg-amber-50 border-b border-amber-200">
						<div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-3">
							<div className="flex items-start gap-3">
								<AlertTriangle className="h-5 w-5 text-amber-600 mt-0.5 flex-shrink-0" />
								<div>
									<h4 className="text-amber-800 font-medium">
										{t("historicalRecord", "Registro Histórico")} - {statusText}
									</h4>
									<p className="text-amber-700 text-sm mt-1">
										{dateText && (
											<span>
												{handoverData.stateName === "Cancelled" 
													? t("cancelledOn", "Cancelado el") 
													: t("completedOn", "Completado el")
												} {dateText}.{" "}
											</span>
										)}
										{t("historicalDescription", "Los datos de este handover no pueden ser modificados.")}
									</p>
								</div>
							</div>
						</div>
					</div>

					{/* Header */}
					<Header onBack={handleBack} />

					{/* Main Content - TODO: Pass readOnly prop when implemented */}
					<div className="px-4 sm:px-6 lg:px-8 py-6 sm:py-8 pb-24">
						<div className="max-w-7xl mx-auto">
							<MainContent />
						</div>
					</div>

					{/* Back to Active Handover Button */}
					<div className="fixed bottom-6 left-1/2 transform -translate-x-1/2 z-50">
						<Button
							variant="default"
							size="lg"
							className="shadow-lg"
							onClick={handleBack}
						>
							<ArrowLeft className="w-4 h-4 mr-2" />
							{t("backToActiveHandover", "Volver al handover activo")}
						</Button>
					</div>
				</SidebarInset>

				{/* Desktop Collaboration Sidebar - Right side (Read-only) */}
				{showComments && !isMobile && (
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
			</SidebarProvider>
		</TooltipProvider>
	);
}
