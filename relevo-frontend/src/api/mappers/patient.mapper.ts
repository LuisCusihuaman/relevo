/**
 * Patient mappers - Transform API types to domain types
 * Rule: Concise-FP - Functional, no classes
 */
import type {
	ApiPatientRecord,
	ApiPatientSummaryCard,
	ApiGetPatientByIdResponse,
	ApiGetPatientHandoverDataResponse,
	ApiPhysicianDto,
} from "@/api/generated";
import type {
	Patient,
	PatientSummaryCard,
	PatientDetail,
	PatientHandoverData,
	PhysicianAssignment,
	IllnessSeverity,
	ShiftCheckInPatient,
	ShiftCheckInStatus,
} from "@/types/domain";

function parseIllnessSeverity(value: string | null | undefined): IllnessSeverity {
	const normalized = value?.toLowerCase();
	if (normalized === "stable" || normalized === "watcher" || normalized === "unstable") {
		return normalized;
	}
	return "stable";
}

function parseShiftCheckInStatus(value: string | null | undefined): ShiftCheckInStatus {
	if (value === "pending" || value === "in-progress" || value === "complete") {
		return value;
	}
	return "pending";
}

export function mapApiPatientToPatient(api: ApiPatientRecord): Patient {
	return {
		id: api.id,
		name: api.name,
		mrn: "",
		room: api.room,
		diagnosis: api.diagnosis,
		age: api.age ?? undefined,
		illnessSeverity: parseIllnessSeverity(api.severity),
	};
}

export function mapApiPatientSummaryCard(api: ApiPatientSummaryCard): PatientSummaryCard {
	return {
		id: api.id,
		name: api.name,
		handoverStatus: api.handoverStatus,
		handoverId: api.handoverId ?? null,
	};
}

export function mapApiPatientDetail(api: ApiGetPatientByIdResponse): PatientDetail {
	return {
		id: api.id,
		name: api.name,
		mrn: api.mrn,
		dob: api.dob,
		gender: api.gender as PatientDetail["gender"],
		admissionDate: api.admissionDate,
		currentUnit: api.currentUnit,
		roomNumber: api.roomNumber,
		diagnosis: api.diagnosis,
		allergies: api.allergies,
		medications: api.medications,
		notes: api.notes,
	};
}

function mapPhysician(api: ApiPhysicianDto | null | undefined): PhysicianAssignment | null {
	if (!api) return null;
	return {
		name: api.name,
		role: api.role,
		color: api.color,
		shiftEnd: api.shiftEnd ?? undefined,
		shiftStart: api.shiftStart ?? undefined,
		status: api.status,
		patientAssignment: api.patientAssignment,
	};
}

export function mapApiPatientHandoverData(api: ApiGetPatientHandoverDataResponse): PatientHandoverData {
	return {
		id: api.id,
		name: api.name,
		mrn: api.mrn,
		dob: api.dob,
		admissionDate: api.admissionDate,
		room: api.room,
		unit: api.unit,
		currentDateTime: api.currentDateTime,
		primaryTeam: api.primaryTeam,
		primaryDiagnosis: api.primaryDiagnosis,
		diagnosis: api.primaryDiagnosis,
		assignedPhysician: mapPhysician(api.assignedPhysician),
		receivingPhysician: mapPhysician(api.receivingPhysician),
		illnessSeverity: parseIllnessSeverity(api.illnessSeverity),
		summaryText: api.summaryText ?? undefined,
		lastEditedBy: api.lastEditedBy ?? undefined,
		updatedAt: api.updatedAt ?? undefined,
	};
}

export function mapApiPatientRecordToShiftCheckIn(api: ApiPatientRecord): ShiftCheckInPatient {
	return {
		id: api.id,
		name: api.name,
		status: parseShiftCheckInStatus(api.status),
		severity: parseIllnessSeverity(api.severity),
		room: api.room,
		diagnosis: api.diagnosis,
	};
}
