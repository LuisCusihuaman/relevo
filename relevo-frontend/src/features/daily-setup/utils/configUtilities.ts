import type { UnitConfig, ShiftConfig } from "../types";

export function transformApiUnit(apiUnit: {
	id: string;
	name: string;
	description?: string | null;
}): UnitConfig {
	return {
		id: apiUnit.id,
		name: apiUnit.name,
		description: apiUnit.description ?? "",
	};
}

export function transformApiShift(apiShift: {
	id: string;
	name: string;
	startTime?: string | null;
	endTime?: string | null;
}): ShiftConfig {
	return {
		id: apiShift.id,
		name: apiShift.name,
		time: apiShift.startTime && apiShift.endTime
			? `${apiShift.startTime} - ${apiShift.endTime}`
			: "",
	};
}
