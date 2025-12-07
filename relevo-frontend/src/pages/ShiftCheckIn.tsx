import { memo, type ReactElement, useMemo } from "react";

import { useShifts, useUnits } from "@/api";
import type { ShiftConfig, UnitConfig } from "@/types/domain";

import { ShiftCheckInWizard } from "@/features/shift-check-in";
import { transformApiShift, transformApiUnit } from "@/features/shift-check-in/utils/configUtilities";

function ShiftCheckInComponent(): ReactElement {
	// Fetch from API - using standard React Query destructuring pattern
	const { data: apiUnits } = useUnits();
	const { data: apiShifts } = useShifts();

	const currentUnitsConfig: Array<UnitConfig> = useMemo(
		() => (apiUnits ?? []).map(transformApiUnit),
		[apiUnits]
	);

	const currentShiftsConfig: Array<ShiftConfig> = useMemo(
		() => (apiShifts ?? []).map(transformApiShift),
		[apiShifts]
	);

	return <ShiftCheckInWizard shifts={currentShiftsConfig} units={currentUnitsConfig} />;
}

export const ShiftCheckIn = memo(ShiftCheckInComponent);
