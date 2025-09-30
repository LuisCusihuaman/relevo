export type FunctionComponent = React.ReactElement | null;

type HeroIconSVGProps = React.PropsWithoutRef<React.SVGProps<SVGSVGElement>> &
	React.RefAttributes<SVGSVGElement>;
type IconProps = HeroIconSVGProps & {
	title?: string;
	titleId?: string;
};
export type Heroicon = React.FC<IconProps>;

// NEW TYPES //

// RELEVO - Composable Patient Entity System
// This file contains all patient-related types using a composable architecture

// ========================================
// CORE ALERT SYSTEM
// ========================================

export interface Alert {
	id: string;
	patientId: string;
	type:
	  | "INFECTION_CONTROL"
	  | "ALLERGY"
	  | "ADVERSE_REACTION"
	  | "RELEVANT_PATHOLOGY"
	  | "SPECIFIC_RISK"
	  | "ADMINISTRATIVE"
	  | "OTHER";
	alertCatalogItem: {
	  code: string;
	  description: string;
	};
	observations?: string;
	level: "HIGH" | "MEDIUM" | "INFORMATIONAL";
	status: "ACTIVE" | "INACTIVE" | "VOIDED";
	startDate: string;
	endDate?: string;
	creationDetails: {
	  author: string;
	  timestamp: string;
	  source: string;
	};
  }
  
  export interface Diagnosis {
	primary: string;
	secondary: Array<string>;
  }
  
  // ========================================
  // COMPOSABLE PATIENT MODULES
  // ========================================
  
  // Basic demographic and identification information
  export interface Demographics {
	id: number;
	name: string;
	age?: number;
	mrn?: string;
	room: string;
	admissionDate?: string;
  }
  
  // Patient workflow status and priorities
  export interface Workflow {
	status: "pending" | "in-progress" | "complete";
	illnessSeverity: "stable" | "watcher" | "unstable";
	priority?: "high" | "medium" | "low";
	lastUpdate: string;
	collaborators: number;
  }
  
  export type IllnessSeverity = Workflow["illnessSeverity"];
  
  // Medical information and diagnosis
  export interface Medical {
	diagnosis: Diagnosis;
	alerts: Array<Alert>; // Proper Alert objects instead of simple alertCount
  }
  
  // Care team information
  export interface CareTeam {
	team?: {
	  attending: string;
	  residents?: Array<string>;
	  nurses?: Array<string>;
	  specialists?: Array<string>;
	};
  }
  
  // I-PASS documentation entries and tracking
  export type IPassEntry = {
	id: string;
	type:
	  | "illness_severity"
	  | "patient_summary"
	  | "action_list"
	  | "situation_awareness";
	timestamp: Date;
	author?: string;
	isComplete?: boolean;
  };
  
  export interface Documentation {
	ipassEntries?: Array<IPassEntry>;
	lastIPassUpdate?: {
	  section:
		| "illness_severity"
		| "patient_summary"
		| "action_list"
		| "situation_awareness";
	  timestamp: Date;
	  author: string;
	};
  }
  
  // Historical data for desktop views
  export interface History {
	handoverHistory?: Array<{
	  id: string;
	  timestamp: Date;
	  from: string;
	  to: string;
	  type: "incoming" | "outgoing";
	  status: "completed" | "pending";
	  duration?: number;
	}>;
	recentActivity?: Array<{
	  id: string;
	  timestamp: Date;
	  action: string;
	  author: string;
	  details: string;
	  section?:
		| "illness_severity"
		| "patient_summary"
		| "action_list"
		| "situation_awareness";
	}>;
  }
  
  // ========================================
  // CLINICAL DOCUMENTATION TYPES
  // ========================================
  
  export interface Author {
	name: string;
	role: string;
	specialty: string;
  }
  
  export interface ClinicalEntry {
	id: string;
	type:
	  | "assessment"
	  | "plan"
	  | "progress"
	  | "family_communication"
	  | "discharge_planning"
	  | "procedure"
	  | "consultation";
	title: string;
	content: string;
	author: Author;
	timestamp: Date;
	isPrivate: boolean;
	tags: Array<string>;
	collaborators?: Array<string>;
  }
  
  // ========================================
  // PATIENT TYPE COMPOSITIONS
  // ========================================
  
  // Base patient with core information
  export type BasePatient = Demographics & Workflow & Medical;
  
  // Standard patient for most views (mobile, lists, etc.) - Extended with additional properties
  export type Patient = BasePatient &
	CareTeam &
	Documentation & {
	  unit?: string;
	  assignedTo?: string;
	  bed?: string;
	  handoverStatus?: string;
	  primaryDiagnosis?: string;
	  vitals?: {
		heartRate?: number;
		bloodPressure?: string;
		temperature?: number;
		oxygenSaturation?: number;
		respiratoryRate?: number;
	  };
	  clinicalEntries?: Array<ClinicalEntry>;
	  admission?: {
		date: string;
		reason: string;
		department: string;
	  };
	  milestones?: {
		estimatedDischarge?: string;
		admission?: string;
		lastAssessment?: string;
		nextPlanned?: string;
		expected?: Array<{
		  id: string;
		  title: string;
		  date: string;
		  completed: boolean;
		}>;
		completed?: Array<{
		  id: string;
		  title: string;
		  completedDate: string;
		}>;
		upcoming?: Array<{
		  id: string;
		  title: string;
		  dueDate: string;
		}>;
	  };
	  familyInfo?: {
		contactPerson?: string;
		relationship?: string;
		phone?: string;
		lastContact?: string;
		concerns?: Array<{
		  id: string;
		  concern: string;
		  timestamp: string;
		}>;
	  };
	};
  
  // Enhanced patient for desktop views with full historical data
  export type DesktopPatient = Patient & History;
  
  // Clinical documentation patient with detailed clinical entries
  export type ClinicalPatient = Patient & {
	clinicalEntries?: Array<ClinicalEntry>;
  };
  
  // Enhanced patient card data for complex patient cards
  export interface EnhancedPatientCardData {
	id: number;
	name: string;
	age?: number;
	mrn?: string;
	room: string;
	diagnosis: string;
	description?: string;
	status: "pending" | "in-progress" | "complete";
	illnessSeverity: "stable" | "watcher" | "unstable";
	alerts: Array<Alert>;
	lastUpdate: string;
	collaborators: number;
	admissionDate?: string;
	priority?: "high" | "medium" | "low";
	unit?: string;
	assignedTo?: string;
	doctor?: string;
	completionPercentage?: number;
	vitals?: Record<string, number | string>;
	iPassData?: {
	  illness: string;
	  patientSummary: string;
	  actionList: Array<string>;
	  situationAwareness: Array<string>;
	  synthesis: string;
	};
	integrationData?: {
	  monitoringActive?: boolean;
	  labLastSync?: string;
	  ehrLastSync?: string;
	};
	actions?: Array<{
	  id: string;
	  type: string;
	  text: string;
	  urgent: boolean;
	  severity: string;
	}>;
	handovers?: Array<{
	  timestamp: string;
	  from: string;
	  to: string;
	  status: string;
	}>;
	medications?: Array<{
	  name: string;
	  dosage: string;
	  frequency: string;
	  route: string;
	}>;
	allergies?: Array<{
	  substance: string;
	  reaction: string;
	  severity: string;
	}>;
	handoverHistory?: Array<{
	  timestamp: string;
	  from: string;
	  to: string;
	  status: string;
	}>;
  }
  
  // Setup patient for daily setup workflow
  export interface SetupPatient {
	id: string | number;
	name: string;
	age?: number;
	room: string;
	diagnosis: string;
	status: "pending" | "in-progress" | "complete";
	severity: "stable" | "watcher" | "unstable";
  }
  
  // Utility functions for working with alerts
  export const getActiveAlerts = (alerts: Array<Alert>): Array<Alert> => {
	return alerts.filter((alert) => alert.status === "ACTIVE");
  };
  
  export const getCriticalAlerts = (alerts: Array<Alert>): Array<Alert> => {
	return getActiveAlerts(alerts).filter((alert) => alert.level === "HIGH");
  };
  
  export const getAlertCount = (alerts: Array<Alert>): number => {
	return getActiveAlerts(alerts).length;
  };
  
  export const getCriticalAlertCount = (alerts: Array<Alert>): number => {
	return getCriticalAlerts(alerts).length;
  };
  
  // Helper function to check if patient has critical alerts
  export const hasCriticalAlerts = (patient: { alerts: Array<Alert> }): boolean => {
	return getCriticalAlertCount(patient.alerts) > 0;
  };
  
  // Helper function to get alert summary for patient cards
  export const getAlertSummary = (
	alerts: Array<Alert>,
  ): { critical: number; total: number } => {
	const activeAlerts = getActiveAlerts(alerts);
	const criticalAlerts = getCriticalAlerts(alerts);
  
	return {
	  critical: criticalAlerts.length,
	  total: activeAlerts.length,
	};
  };
  
  // Helper functions for I-PASS documentation
  export const getIPassCompletionStatus = (
	entries?: Documentation["ipassEntries"],
  ) => {
	if (!entries) return { completed: 0, total: 4 };
  
	const requiredSections = [
	  "illness_severity",
	  "patient_summary",
	  "action_list",
	  "situation_awareness",
	];
	const completedSections = requiredSections.filter((section) =>
	  entries.some(
		(entry) => entry.type === section && entry.isComplete !== false,
	  ),
	);
  
	return {
	  completed: completedSections.length,
	  total: requiredSections.length,
	};
  };
  
  export const getLastIPassActivity = (patient: Patient): string => {
	if (patient.lastIPassUpdate) {
	  const sectionNames = {
		illness_severity: "Illness Severity",
		patient_summary: "Patient Summary",
		action_list: "Action List",
		situation_awareness: "Situation Awareness",
	  };
	  return `${sectionNames[patient.lastIPassUpdate.section]} updated`;
	}
  
	if (patient.ipassEntries && patient.ipassEntries.length > 0) {
	  const latest = patient.ipassEntries[patient.ipassEntries.length - 1]!;
	  const sectionNames = {
		illness_severity: "Illness Severity",
		patient_summary: "Patient Summary",
		action_list: "Action List",
		situation_awareness: "Situation Awareness",
	  };
	  return `${sectionNames[latest.type]} documented`;
	}
  
	return "No I-PASS documentation yet";
  };
  
  // ========================================
  // CONFIGURATION TYPES
  // ========================================
  
  export interface UnitConfig {
	id: string;
	name: string;
	description: string;
  }
  
  export interface ShiftConfig {
	id: string;
	name: string;
	time: string;
  }
  
  // ========================================
  // DAILY SETUP TYPES
  // ========================================
  
  export interface RecentActivity {
	id: number;
	type: string;
	patient: string;
	doctor: string;
	timestamp: string;
	details: string;
  }
  
  export interface DailySetupData {
	unit: string;
	shift: string;
	selectedPatients: Array<number>;
	doctorName: string;
  }
  
  // ========================================
  // ADDITIONAL INTERFACES FOR STORE FILES
  // ========================================
  
  // I-PASS Block interface for clinical documentation
  export interface IPassBlock {
	id: string;
	name: string;
	shortName: string;
	icon: string;
	color: string;
	placeholder: string;
	quickActions: Array<QuickAction>;
  }
  
  // Quick action interface for clinical templates
  export interface QuickAction {
	label: string;
	text: string;
  }
  
  // Shift doctor interface for shift management
  export interface ShiftDoctor {
	id?: string;
	name: string;
	specialty: string;
	role: "attending" | "resident" | "fellow";
	unit?: string;
	shift?: "day" | "night" | "evening";
	contact?: string;
	status: "on-duty" | "off-duty" | "on-call";
	shiftStart?: string;
	shiftEnd?: string;
  }
  
  // Doctor metrics interface for user analytics
  export interface DoctorMetrics {
	totalPatients: number;
	completedHandovers: number;
	avgHandoverTime: number;
	ipassCompletionRate: number;
	alertsResolved: number;
	weeklyGoals: {
	  handovers: number;
	  documentation: number;
	  alerts: number;
	};
  }
  
  // HANDOVER
  
  export type SyncStatus = "synced" | "syncing" | "pending" | "offline" | "error";
  
  export type FullscreenComponent = "patient-summary" | "situation-awareness";
  
  export interface FullscreenEditingState {
	component: FullscreenComponent;
	autoEdit: boolean;
  }
  
  export interface ExpandedSections {
	illness: boolean;
	patient: boolean;
	actions: boolean;
	awareness: boolean;
	synthesis: boolean;
  }
  
  export interface User {
	name: string;
	role: string;
	shift: string;
	initials: string;
  }
  
  export interface Physician {
	name: string;
	role: string;
	initials: string;
	color: string;
	shiftEnd?: string;
	shiftStart?: string;
	status: string;
	patientAssignment: string;
  }
  
  export interface PatientData {
	id: string;
	name: string;
	dob: string; // Raw DOB from backend, calculate age in UI
	mrn: string;
	admissionDate: string;
	currentDateTime: string;
	primaryTeam: string;
	primaryDiagnosis: string;
	room: string;
	unit: string;
	assignedPhysician: Physician | null;
	receivingPhysician: Physician | null;
	// Removed: severity, handoverStatus, shift, handoverTime
	// These come from the handover endpoint
  }
  
  export interface Collaborator {
	id: number;
	name: string;
	initials: string;
	color: string;
	status: "active" | "viewing" | "offline";
	lastSeen: string;
	activity: string;
	role: string;
	presenceType:
	  | "assigned-current"
	  | "assigned-receiving"
	  | "participating"
	  | "supporting";
  }
  
  export interface AppState {
	workflowSetup: boolean;
	handoverComplete: boolean;
	showHistory: boolean;
	showComments: boolean;
	showActivityFeed: boolean;
	showCollaborators: boolean;
	showShareMenu: boolean;
	showMobileMenu: boolean;
	fullscreenEditing: FullscreenEditingState | null;
	isOnline: boolean;
	syncStatus: SyncStatus;
	layoutMode: "single" | "columns";
	sessionDuration: number;
	expandedSections: ExpandedSections;
	currentSaveFunction: (() => void) | null;
  }
  