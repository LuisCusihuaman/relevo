import { memo, type ReactElement, useMemo } from "react";

import { useShifts, useUnits } from "@/api";
import type { ShiftConfig, UnitConfig } from "@/common/types";
import { SetupWizard } from "@/features/daily-setup";
import { transformApiShift, transformApiUnit } from "@/features/daily-setup/utils/configUtilities";

function DailySetupComponent(): ReactElement {
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

	return <SetupWizard shifts={currentShiftsConfig} units={currentUnitsConfig} />;
}

export const DailySetup = memo(DailySetupComponent);
