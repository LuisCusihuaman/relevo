/**
 * Shift Check-In mappers - Transform API types to domain types
 * Rule: Concise-FP - Functional, no classes
 */
import type { ApiShiftRecord, ApiUnitRecord } from "@/api/generated";
import type { Shift, Unit } from "@/types/domain";

export function mapApiUnit(api: ApiUnitRecord): Unit {
	return {
		id: api.id ?? "",
		name: api.name ?? "",
	};
}

export function mapApiShift(api: ApiShiftRecord): Shift {
	return {
		id: api.id ?? "",
		name: api.name ?? "",
		startTime: api.startTime ?? undefined,
		endTime: api.endTime ?? undefined,
	};
}
