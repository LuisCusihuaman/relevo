/**
 * Handover mappers - Transform API types to domain types
 * Rule: Concise-FP - Functional, no classes
 */
import type {
	ApiHandoverRecord,
	ApiHandoverDto,
	ApiGetHandoverByIdResponse,
	ApiHandoverActionItemFullRecord,
	ApiContingencyPlanRecord,
	ApiContingencyPlanDto,
	ApiSituationAwarenessDto,
	ApiSynthesisDto,
	ApiGetClinicalDataResponse,
} from "@/api/generated";
import type {
	HandoverSummary,
	HandoverDetail,
	HandoverActionItem,
	ContingencyPlan,
	SituationAwareness,
	Synthesis,
	ClinicalData,
	IllnessSeverity,
	HandoverStatus,
	SituationAwarenessStatus,
	Priority,
	ContingencyStatus,
} from "@/types/domain";

function parseIllnessSeverity(value: string | null | undefined): IllnessSeverity {
	const normalized = value?.toLowerCase();
	if (normalized === "stable" || normalized === "watcher" || normalized === "unstable") {
		return normalized;
	}
	return "stable";
}

function parseHandoverStatus(value: string | null | undefined): HandoverStatus {
	const valid: Array<HandoverStatus> = [
		"Draft",
		"Ready",
		"InProgress",
		"Completed",
		"Cancelled",
	];
	if (value && valid.includes(value as HandoverStatus)) {
		return value as HandoverStatus;
	}
	return "Draft";
}

function parseSituationAwarenessStatus(value: string | null | undefined): SituationAwarenessStatus {
	const valid: Array<SituationAwarenessStatus> = ["Draft", "Ready", "InProgress", "Completed"];
	if (value && valid.includes(value as SituationAwarenessStatus)) {
		return value as SituationAwarenessStatus;
	}
	return "Draft";
}

function parsePriority(value: string | null | undefined): Priority {
	if (value === "low" || value === "medium" || value === "high") {
		return value;
	}
	return "medium";
}

function parseContingencyStatus(value: string | null | undefined): ContingencyStatus {
	if (value === "active" || value === "planned" || value === "completed") {
		return value;
	}
	return "active";
}

export function mapApiHandoverRecord(api: ApiHandoverRecord): HandoverSummary {
	return {
		id: api.id,
		patientId: api.patientId,
		patientName: api.patientName ?? null,
		shiftName: api.shiftName,
		stateName: parseHandoverStatus(api.stateName),
		illnessSeverity: parseIllnessSeverity(api.illnessSeverity),
		createdBy: api.createdBy,
		createdByName: api.createdByName ?? null,
		assignedTo: api.assignedTo,
		assignedToName: api.assignedToName ?? null,
		responsiblePhysicianName: api.responsiblePhysicianName,
		createdAt: api.createdAt ?? undefined,
		completedAt: api.completedAt ?? undefined,
	};
}

/** Maps HandoverDto (limited fields from pending endpoint) to HandoverSummary with defaults */
export function mapApiHandoverDto(api: ApiHandoverDto): HandoverSummary {
	return {
		id: api.id ?? "",
		patientId: api.patientId ?? "",
		patientName: api.patientName ?? null,
		shiftName: api.shiftName ?? "",
		stateName: parseHandoverStatus(api.status),
		illnessSeverity: "stable",
		createdBy: "",
		createdByName: null,
		assignedTo: "",
		assignedToName: null,
		responsiblePhysicianName: "",
	};
}

export function mapApiHandoverDetail(api: ApiGetHandoverByIdResponse): HandoverDetail {
	return {
		id: api.id,
		patientId: api.patientId,
		patientName: api.patientName ?? null,
		shiftName: api.shiftName,
		stateName: parseHandoverStatus(api.stateName),
		illnessSeverity: parseIllnessSeverity(api.illnessSeverity.severity),
		patientSummaryContent: api.patientSummary.content,
		synthesisContent: api.synthesis?.content ?? null,
		responsiblePhysicianId: api.responsiblePhysicianId,
		responsiblePhysicianName: api.responsiblePhysicianName,
		createdBy: api.createdBy,
		assignedTo: api.assignedTo,
		receiverUserId: api.receiverUserId ?? undefined,
		createdAt: api.createdAt ?? undefined,
		readyAt: api.readyAt ?? undefined,
		startedAt: api.startedAt ?? undefined,
		completedAt: api.completedAt ?? undefined,
		cancelledAt: api.cancelledAt ?? undefined,
		version: api.version,
		shiftWindowId: api.shiftWindowId ?? undefined,
		previousHandoverId: api.previousHandoverId ?? undefined,
		cancelReason: api.cancelReason ?? undefined,
	};
}

export function mapApiActionItem(api: ApiHandoverActionItemFullRecord): HandoverActionItem {
	return {
		id: api.id,
		handoverId: api.handoverId,
		description: api.description,
		isCompleted: api.isCompleted,
		createdAt: api.createdAt,
		updatedAt: api.updatedAt,
		completedAt: api.completedAt ?? null,
	};
}

export function mapApiContingencyPlan(api: ApiContingencyPlanRecord | ApiContingencyPlanDto): ContingencyPlan {
	return {
		id: api.id ?? "",
		handoverId: api.handoverId ?? "",
		conditionText: api.conditionText ?? "",
		actionText: api.actionText ?? "",
		priority: parsePriority(api.priority),
		status: parseContingencyStatus(api.status),
		createdBy: api.createdBy ?? "",
		createdAt: api.createdAt ?? "",
		updatedAt: api.updatedAt ?? "",
	};
}

export function mapApiSituationAwareness(api: ApiSituationAwarenessDto | null): SituationAwareness | null {
	if (!api) return null;
	return {
		handoverId: api.handoverId,
		content: api.content ?? null,
		status: parseSituationAwarenessStatus(api.status),
		lastEditedBy: api.lastEditedBy,
		updatedAt: api.updatedAt,
	};
}

export function mapApiSynthesis(api: ApiSynthesisDto | null): Synthesis | null {
	if (!api) return null;
	return {
		handoverId: api.handoverId,
		content: api.content ?? null,
		status: api.status,
		lastEditedBy: api.lastEditedBy,
		updatedAt: api.updatedAt,
	};
}

export function mapApiClinicalData(api: ApiGetClinicalDataResponse): ClinicalData {
	return {
		handoverId: api.handoverId,
		illnessSeverity: parseIllnessSeverity(api.illnessSeverity),
		summaryText: api.summaryText,
		lastEditedBy: api.lastEditedBy,
		status: api.status,
		updatedAt: api.updatedAt,
	};
}
