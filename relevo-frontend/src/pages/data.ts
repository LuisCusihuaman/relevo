
import type {
	Handover,
	Metric,
	Patient,
	RecentPreview,
	SearchResult,
} from "../components/home/types";

export const patients: Array<Patient> = [
	{
		id: "ana-perez",
		name: "Ana Pérez",
		url: "relevo-app.vercel.app",
		status: "home:patientList.startHandover",
		date: "Jul 12",
		icon: "A",
		unit: "UCI",
		handoverId: "CDMFQKRs",
	},
	{
		id: "juan-rodriguez",
		name: "Juan Rodríguez",
		url: "v0-portfolio-template-by-v0-mu-...",
		status: "home:patientList.startHandover",
		date: "Aug 17",
		icon: "J",
		unit: "Cardiología",
		handoverId: null,
	},
	{
		id: "carlos-gomez",
		name: "Carlos Gómez",
		url: "Sin traspaso activo",
		status: "home:patientList.noActiveHandover",
		date: "2/22/21",
		icon: "C",
		unit: "Emergencia",
		handoverId: "3L5k6ngCp",
	},
	{
		id: "maria-garcia",
		name: "María García",
		url: "Sin traspaso activo",
		status: "home:patientList.pendingClinicalNotes",
		date: "8/8/20",
		icon: "M",
		unit: "Pediatría",
		handoverId: null,
	},
	{
		id: "laura-schmidt",
		name: "Laura Schmidt",
		url: "psa-frontend-alpha.vercel.app",
		status: "home:patientList.startHandover",
		date: "6/30/24",
		icon: "L",
		unit: "UCI",
		handoverId: "6qYUWvuN3",
	},
	{
		id: "pedro-martinez",
		name: "Pedro Martinez",
		url: "image-component-sandy-three.v...",
		status: "home:patientList.initialAdmission",
		date: "2/5/21",
		icon: "P",
		unit: "Radiología",
		handoverId: null,
	},
	{
		id: "sofia-rossi",
		name: "Sofía Rossi",
		url: "v0-music-game-concept.vercel.a...",
		status: "home:patientList.startHandover",
		date: "Mar 25",
		icon: "S",
		unit: "Psiquiatría",
		handoverId: null,
	},
	{
		id: "martin-herrera",
		name: "Martin Herrera",
		url: "backoffice-pi-dusky.vercel.app",
		status: "home:patientList.initialAdmission",
		date: "10/16/23",
		icon: "M",
		unit: "Administración",
		handoverId: null,
	},
];

// Removed mapping; names are stored per handover as patientName.

export const recentPreviews: Array<RecentPreview> = [
	{
			title: "home:recentPreview.newPatientAssigned",
		avatars: [
			{ src: null, fallback: "LC", bg: "bg-blue-500" },
			{ src: null, fallback: "JD", bg: "bg-green-500" },
		],
		status: "Source",
		pr: "#123",
		color: "Ready",
	},
	{
		title: "home:recentPreview.severityChangedCritical",
		avatars: [{ src: null, fallback: "SM", bg: "bg-purple-500" }],
		status: "Error",
		pr: "#124",
	},
	{
		title: "home:recentPreview.newActionAssigned",
		avatars: [
			{ src: null, fallback: "AB", bg: "bg-orange-500" },
			{ src: null, fallback: "CD", bg: "bg-pink-500" },
		],
		status: "Source",
		pr: "#125",
		color: "Ready",
	},
	{
		title: "home:recentPreview.handoverComment",
		avatars: [{ src: null, fallback: "LC", bg: "bg-blue-500" }],
		status: "Source",
		pr: "#126",
		color: "Ready",
	},
	{
		title: "home:recentPreview.handoverCompleted",
		avatars: [{ src: null, fallback: "JD", bg: "bg-green-500" }],
		status: "Source",
		pr: "#127",
		color: "Ready",
	},
];

export const handovers: Array<Handover> = [
	{
		id: "CDMFQKRs",
		status: "Error",
		statusColor: "bg-red-500",
		environment: "Unexpected Error",
		environmentColor: "text-red-600",
		patientKey: "ana-perez",
		patientName: "Ana Pérez",
		patientIcon: {
			type: "text",
			value: "A",
			bg: "bg-blue-100",
			text: "text-gray-700",
		},
		time: "2d ago",
		statusTime: "2s (2d ago)",
		environmentType: "Preview",
		bedLabel: "302A",
		mrn: "892778",
	},
	{
		id: "8LB4tSUAh",
		status: "Error",
		statusColor: "bg-red-500",
		environment: "Unexpected Error",
		environmentColor: "text-red-600",
		patientKey: "ana-perez",
		patientName: "Ana Pérez",
		patientIcon: {
			type: "text",
			value: "A",
			bg: "bg-blue-100",
			text: "text-gray-700",
		},
		time: "3d ago",
		statusTime: "2s (3d ago)",
		environmentType: "Preview",
		mrn: "12345678",
	},
	{
		id: "3L5k6ngCp",
		status: "Error",
		statusColor: "bg-red-500",
		environment: "Unexpected Error",
		environmentColor: "text-red-600",
		patientKey: "carlos-gomez",
		patientName: "Carlos Gómez",
		patientIcon: {
			type: "text",
			value: "C",
			bg: "bg-blue-100",
			text: "text-gray-700",
		},
		time: "Aug 6",
		statusTime: "4s (18d ago)",
		environmentType: "Preview",
		bedLabel: "217B",
	},
	{
		id: "GX6A8fhaZ",
		status: "Error",
		statusColor: "bg-red-500",
		environment: "Unexpected Error",
		environmentColor: "text-red-600",
		patientKey: "ana-perez",
		patientName: "Ana Pérez",
		patientIcon: {
			type: "text",
			value: "A",
			bg: "bg-blue-100",
			text: "text-gray-700",
		},
		time: "Jul 17",
		statusTime: "4s (38d ago)",
		environmentType: "Preview",
		mrn: "99887766",
	},
	{
		id: "6qYUWvuN3",
		status: "Ready",
		statusColor: "bg-green-500",
		environment: "Promoted",
		environmentColor: "text-gray-600",
		patientKey: "laura-schmidt",
		patientName: "Laura Schmidt",
		patientIcon: {
			type: "text",
			value: "L",
			bg: "bg-purple-500",
			text: "text-white",
		},
		time: "Jul 12",
		statusTime: "29s (43d ago)",
		current: true,
		environmentType: "Production",
		bedLabel: "401C",
	},
	{
		id: "8TdfXLHgY",
		status: "Ready",
		statusColor: "bg-green-500",
		environment: "Staged",
		environmentColor: "text-gray-600",
		patientKey: "laura-schmidt",
		patientName: "Laura Schmidt",
		patientIcon: {
			type: "text",
			value: "L",
			bg: "bg-purple-500",
			text: "text-white",
		},
		time: "Jul 12",
		statusTime: "21s (43d ago)",
		environmentType: "Preview",
		mrn: "445566",
	},
];

export const searchResults: Array<SearchResult> = [
	// Pacientes
	{ name: "Ana Pérez", category: "home:search.category.patients", type: "patient" },
	{ name: "Juan Rodríguez", category: "home:search.category.patients", type: "patient" },
	{ name: "Carlos Gómez", category: "home:search.category.patients", type: "patient" },


	// Pacientes (antes Traspasos)
	{ name: "Ana Pérez", category: "home:search.category.patients", type: "handover" },
	{ name: "Laura Schmidt", category: "home:search.category.patients", type: "handover" },

	// Acciones rápidas (acciones de comando)
	{ name: "home:search.actions.startHandover", category: "home:search.category.quickActions", type: "assistant" },
	{ name: "home:search.actions.addAction", category: "home:search.category.quickActions", type: "assistant" },
	{ name: "home:search.actions.markCriticalAlert", category: "home:search.category.quickActions", type: "assistant" },
	{ name: "home:search.actions.openNotifications", category: "home:search.category.quickActions", type: "assistant" },
	{ name: "home:search.actions.createClinicalNote", category: "home:search.category.quickActions", type: "assistant" },

	// Notas clínicas
	{ name: "Nota de Ana Pérez", category: "home:search.category.clinicalNotes", type: "assistant" },
	{ name: "Nota de Carlos Gómez", category: "home:search.category.clinicalNotes", type: "assistant" },

	// Unidades/Servicios
	{ name: "UCI", category: "home:search.category.units", type: "assistant" },
	{ name: "Emergencia", category: "home:search.category.units", type: "assistant" },

	// Personas/Equipo
	{ name: "Luis Cusihuaman", category: "home:search.category.team", type: "team" },
];

export const metrics: Array<Metric> = [
	{
		label: "home:metrics.assignedPatients.label",
		value: "12",
		tooltip: "home:metrics.assignedPatients.tooltip",
		currentValue: "12",
		totalValue: "15",
	},
	{
		label: "home:metrics.handoversInProgress.label",
		value: "5",
		tooltip: "home:metrics.handoversInProgress.tooltip",
		currentValue: "5",
		totalValue: "10",
	},
	{
		label: "home:metrics.pendingActions.label",
		value: "18",
		tooltip: "home:metrics.pendingActions.tooltip",
		currentValue: "18",
		totalValue: "25",
	},
	{
		label: "home:metrics.criticalAlerts.label",
		value: "2",
		tooltip: "home:metrics.criticalAlerts.tooltip",
		currentValue: "2",
		totalValue: "3",
	},
];
