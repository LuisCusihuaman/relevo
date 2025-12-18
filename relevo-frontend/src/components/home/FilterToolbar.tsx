import type { FC } from "react";
import { useTranslation } from "react-i18next";

type FilterToolbarProps = {
	selectedUnit?: string | null;
	onUnitChange?: (unitId: string | null) => void;
};

export const FilterToolbar: FC<FilterToolbarProps> = ({
	selectedUnit,
	onUnitChange,
}) => {
	const { t } = useTranslation("home");

	const handleUnitChange = (event: React.ChangeEvent<HTMLSelectElement>): void => {
		event.preventDefault();
		const value = event.target.value;
		onUnitChange?.(value === "all" ? null : value);
	};

	return (
		<div className="flex items-center justify-between mt-6 mb-4">
			<div className="flex items-center gap-4">
				<select
					className="px-3 py-2 border border-gray-300 rounded-lg text-sm bg-white"
					value={selectedUnit ?? "all"}
					onChange={handleUnitChange}
				>
					<option value="all">{t("filterToolbar.allUnits")}</option>
					<option value="unit-1">{t("filterToolbar.icu")}</option>
					<option value="unit-2">{t("filterToolbar.generalPediatrics")}</option>
				</select>
			</div>
			<div className="flex items-center gap-4">
				<select className="px-3 py-2 border border-gray-300 rounded-lg text-sm bg-white">
					<option>{t("filterToolbar.status")}</option>
					<option>{t("filterToolbar.all")}</option>
					<option>{t("filterToolbar.notStarted")}</option>
					<option>{t("filterToolbar.inProgress")}</option>
					<option>{t("filterToolbar.completed")}</option>
					<option>{t("filterToolbar.failed")}</option>
				</select>
			</div>
		</div>
	);
};
