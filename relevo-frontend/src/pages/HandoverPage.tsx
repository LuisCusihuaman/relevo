import { type ReactElement, useMemo } from "react";
import { useParams, useNavigate } from "@tanstack/react-router";
import { useTranslation } from "react-i18next";
import { useHandover } from "@/api/endpoints/handovers";
import { usePatientDetails } from "@/api/endpoints/patients";
import { mapApiHandoverToUiHandover } from "@/api/mappers";
import type { Handover } from "@/components/home/types";
import { ArrowLeft, Users, Clock, CheckCircle } from "lucide-react";

export function HandoverPage(): ReactElement {
	const { t } = useTranslation();
	const navigate = useNavigate();
	// eslint-disable-next-line @typescript-eslint/no-unsafe-assignment
	const { handoverId } = useParams({
		from: "/_authenticated/$patientSlug/$handoverId",
	});

	const { data: apiHandover, isLoading: handoverLoading, error: handoverError } = useHandover(String(handoverId));
	const { data: patientData, isLoading: patientLoading, error: patientError } = usePatientDetails(apiHandover?.patientId || "");

	const isLoading = handoverLoading || patientLoading;
	const error = handoverError || patientError;

	// Combine handover and patient data
	const handover: Handover | undefined = useMemo(() => {
		if (!apiHandover || !patientData) return undefined;

		const uiHandover = mapApiHandoverToUiHandover(apiHandover);
		// Override patient name with actual patient data
		return {
			...uiHandover,
			patientName: patientData.name,
			patientIcon: {
				...uiHandover.patientIcon,
				value: patientData.name.charAt(0).toUpperCase(),
			},
			bedLabel: patientData.roomNumber,
			mrn: patientData.mrn,
		};
	}, [apiHandover, patientData]);

	const handleBack = (): void => {
		void navigate({ to: "/patients" });
	};

	const formatTime = (dateString: string): string => {
		try {
			// Handle different date formats from backend
			let date: Date;

			if (dateString.includes('T')) {
				// ISO format: 2025-09-20T17:10:50
				date = new Date(dateString);
			} else if (dateString.includes(' ')) {
				// Oracle format: 2025-09-20 17:10:50
				// Convert to ISO format for better parsing
				date = new Date(dateString.replace(' ', 'T'));
			} else {
				// Fallback for other formats
				date = new Date(dateString);
			}

			if (isNaN(date.getTime())) {
				// If invalid date, return current time
				return new Date().toLocaleTimeString("es-ES", {
					hour: "2-digit",
					minute: "2-digit",
				});
			}
			return date.toLocaleTimeString("es-ES", {
				hour: "2-digit",
				minute: "2-digit",
			});
		} catch {
			// Fallback to current time if parsing fails
			return new Date().toLocaleTimeString("es-ES", {
				hour: "2-digit",
				minute: "2-digit",
			});
		}
	};

	const getStatusColor = (status: string): string => {
		switch (status) {
			case "Ready":
				return "text-green-600 bg-green-50";
			case "Error":
				return "text-red-600 bg-red-50";
			default:
				return "text-gray-600 bg-gray-50";
		}
	};

	// Show loading state
	if (isLoading) {
		return (
			<div className="min-h-screen bg-gray-50 flex items-center justify-center">
				<div className="text-center">
					<div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto mb-4"></div>
					<p className="text-gray-600">Cargando traspaso...</p>
				</div>
			</div>
		);
	}

	// Show error state
	if (error) {
		return (
			<div className="min-h-screen bg-gray-50 flex items-center justify-center">
				<div className="text-center">
					<div className="text-red-600 mb-4">
						{t("patientList.errorLoadingHandover", { defaultValue: "Error al cargar el traspaso" })}
					</div>
					<p className="text-gray-600 mb-4">
						{t("patientList.tryReload", { defaultValue: "Intente recargar la página" })}
					</p>
					<button
						className="bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700"
						onClick={handleBack}
					>
						{t("patientList.backToPatients", { defaultValue: "Volver a pacientes" })}
					</button>
				</div>
			</div>
		);
	}

	if (!handover) {
		return (
			<div className="min-h-screen bg-gray-50 flex items-center justify-center">
				<div className="text-center">
					<div className="text-gray-600 mb-4">
						{t("patientList.handoverNotFound", { defaultValue: "Traspaso no encontrado" })}
					</div>
					<button
						className="bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700"
						onClick={handleBack}
					>
						{t("patientList.backToPatients", { defaultValue: "Volver a pacientes" })}
					</button>
				</div>
			</div>
		);
	}

	return (
		<div className="min-h-screen bg-gray-50">
			{/* Header */}
			<header className="bg-white shadow-sm border-b">
				<div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
					<div className="flex items-center justify-between h-16">
						<div className="flex items-center gap-4">
							<button
								className="flex items-center gap-2 text-gray-600 hover:text-gray-900 transition-colors"
								onClick={handleBack}
							>
								<ArrowLeft className="h-5 w-5" />
								{t("patientList.back", { defaultValue: "Volver" })}
							</button>
							<div className="flex items-center gap-3">
								<div className="h-10 w-10 rounded-full bg-blue-100 flex items-center justify-center">
									{handover.patientIcon?.value || handover.patientName.charAt(0)}
								</div>
								<div>
									<h1 className="text-xl font-semibold text-gray-900">
										{handover.patientName}
									</h1>
									<p className="text-sm text-gray-600">
										{t("handover.focusTitle", { defaultValue: "Sesión de Traspaso I-PASS" })}
									</p>
								</div>
							</div>
						</div>

						<div className="flex items-center gap-4">
							<div className="flex items-center gap-2">
								<Clock className="h-4 w-4 text-gray-500" />
								<span className="text-sm text-gray-600">
									{t("handover.handoverAt", {
										time: formatTime(handover.time),
										defaultValue: "Traspaso a las {{time}}"
									})}
								</span>
							</div>
							<div className={`px-3 py-1 rounded-full text-sm font-medium ${getStatusColor(handover.status)}`}>
								{handover.status}
							</div>
							<div className="flex items-center gap-2 text-sm text-gray-600">
								<Users className="h-4 w-4" />
								{t("handover.participants", {
									count: 1,
									defaultValue: "{{count}} participante"
								})}
							</div>
						</div>
					</div>
				</div>
			</header>

			{/* Main Content */}
			<main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
				<div className="grid grid-cols-1 lg:grid-cols-4 gap-8">
					{/* I-PASS Content */}
					<div className="lg:col-span-3 space-y-8">
						{/* Illness Severity */}
						<div className="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
							<h2 className="text-lg font-semibold text-gray-900 mb-4">
								{t("handover.ipassGuidelines.illness.title", { defaultValue: "Guías de Severidad de Enfermedad" })}
							</h2>
							<div className="space-y-3">
								{Object.entries(t("handover.ipassGuidelines.illness.points", {
									returnObjects: true,
									defaultValue: {}
								}) as Array<[string, unknown]>).map(([key, point]) => (
									<div key={key} className="flex items-start gap-3">
										<div className="h-6 w-6 rounded-full bg-red-100 flex items-center justify-center flex-shrink-0 mt-0.5">
											<span className="text-xs font-medium text-red-600">{parseInt(key) + 1}</span>
										</div>
										<p className="text-sm text-gray-700">{String(point)}</p>
									</div>
								))}
							</div>
						</div>

						{/* Patient Summary */}
						<div className="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
							<h2 className="text-lg font-semibold text-gray-900 mb-4">
								{t("handover.ipassGuidelines.patient.title", { defaultValue: "Guías de Resumen del Paciente" })}
							</h2>
							<div className="space-y-3">
								{Object.entries(t("handover.ipassGuidelines.patient.points", {
									returnObjects: true,
									defaultValue: {}
								}) as Array<[string, unknown]>).map(([key, point]) => (
									<div key={key} className="flex items-start gap-3">
										<div className="h-6 w-6 rounded-full bg-blue-100 flex items-center justify-center flex-shrink-0 mt-0.5">
											<span className="text-xs font-medium text-blue-600">{parseInt(key) + 1}</span>
										</div>
										<p className="text-sm text-gray-700">{String(point)}</p>
									</div>
								))}
							</div>
						</div>

						{/* Action List */}
						<div className="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
							<h2 className="text-lg font-semibold text-gray-900 mb-4">
								{t("handover.ipassGuidelines.actions.title", { defaultValue: "Guías de Lista de Acciones" })}
							</h2>
							<div className="space-y-3">
								{Object.entries(t("handover.ipassGuidelines.actions.points", {
									returnObjects: true,
									defaultValue: {}
								}) as Array<[string, unknown]>).map(([key, point]) => (
									<div key={key} className="flex items-start gap-3">
										<div className="h-6 w-6 rounded-full bg-green-100 flex items-center justify-center flex-shrink-0 mt-0.5">
											<span className="text-xs font-medium text-green-600">{parseInt(key) + 1}</span>
										</div>
										<p className="text-sm text-gray-700">{String(point)}</p>
									</div>
								))}
							</div>
						</div>

						{/* Situation Awareness */}
						<div className="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
							<h2 className="text-lg font-semibold text-gray-900 mb-4">
								{t("handover.ipassGuidelines.awareness.title", { defaultValue: "Guías de Conciencia Situacional" })}
							</h2>
							<div className="space-y-3">
								{Object.entries(t("handover.ipassGuidelines.awareness.points", {
									returnObjects: true,
									defaultValue: {}
								}) as Array<[string, unknown]>).map(([key, point]) => (
									<div key={key} className="flex items-start gap-3">
										<div className="h-6 w-6 rounded-full bg-yellow-100 flex items-center justify-center flex-shrink-0 mt-0.5">
											<span className="text-xs font-medium text-yellow-600">{parseInt(key) + 1}</span>
										</div>
										<p className="text-sm text-gray-700">{String(point)}</p>
									</div>
								))}
							</div>
						</div>

						{/* Synthesis */}
						<div className="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
							<h2 className="text-lg font-semibold text-gray-900 mb-4">
								{t("handover.ipassGuidelines.synthesis.title", { defaultValue: "Guías de Síntesis" })}
							</h2>
							<div className="space-y-3">
								{Object.entries(t("handover.ipassGuidelines.synthesis.points", {
									returnObjects: true,
									defaultValue: {}
								}) as Array<[string, unknown]>).map(([key, point]) => (
									<div key={key} className="flex items-start gap-3">
										<div className="h-6 w-6 rounded-full bg-purple-100 flex items-center justify-center flex-shrink-0 mt-0.5">
											<span className="text-xs font-medium text-purple-600">{parseInt(key) + 1}</span>
										</div>
										<p className="text-sm text-gray-700">{String(point)}</p>
									</div>
								))}
							</div>
						</div>
					</div>

					{/* Sidebar */}
					<div className="space-y-6">
						{/* Session Info */}
						<div className="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
							<h3 className="text-sm font-medium text-gray-900 mb-3">
								{t("handover.session", { duration: "45 min", defaultValue: "Sesión: {{duration}}" })}
							</h3>
							<div className="space-y-2">
								<div className="flex justify-between text-sm">
									<span className="text-gray-600">
										{t("handover.remaining", { time: "32 min", defaultValue: "{{time}} restantes" })}
									</span>
									<span className="text-gray-900 font-medium">70%</span>
								</div>
								<div className="w-full bg-gray-200 rounded-full h-2">
									<div className="bg-blue-600 h-2 rounded-full" style={{ width: "70%" }}></div>
								</div>
							</div>
						</div>

						{/* Complete Handover Button */}
						<div className="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
							<button className="w-full bg-green-600 text-white px-4 py-3 rounded-lg hover:bg-green-700 transition-colors flex items-center justify-center gap-2 font-medium">
								<CheckCircle className="h-5 w-5" />
								{t("handover.completeHandover", { defaultValue: "Completar Traspaso" })}
							</button>
							<p className="text-xs text-gray-600 mt-2 text-center">
								{t("handover.handoverComplete", { defaultValue: "Traspaso completado" })}
							</p>
						</div>

						{/* Environment Info */}
						<div className="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
							<h3 className="text-sm font-medium text-gray-900 mb-3">
								{t("patientList.environment", { defaultValue: "Entorno" })}
							</h3>
							<div className="space-y-2">
								<div className="flex justify-between text-sm">
									<span className="text-gray-600">{handover.environment}</span>
									<span className={`px-2 py-1 rounded text-xs font-medium ${handover.environmentColor}`}>
										{handover.environmentType}
									</span>
								</div>
								{handover.bedLabel && (
									<div className="flex justify-between text-sm">
										<span className="text-gray-600">
											{t("patientList.bed", { defaultValue: "Cama" })}
										</span>
										<span className="text-gray-900">{handover.bedLabel}</span>
									</div>
								)}
								{handover.mrn && (
									<div className="flex justify-between text-sm">
										<span className="text-gray-600">MRN</span>
										<span className="text-gray-900">{handover.mrn}</span>
									</div>
								)}
							</div>
						</div>
					</div>
				</div>
			</main>
		</div>
	);
}
