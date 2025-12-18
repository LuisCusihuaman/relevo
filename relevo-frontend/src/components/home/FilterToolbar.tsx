import type { FC } from "react";
import { useTranslation } from "react-i18next";

type FilterToolbarProps = {
	selectedUnit?: string | null;
	onUnitChange?: (unitId: string | null) => void;
	selectedUser?: string | null;
	onUserChange?: (userId: string | null) => void;
	users?: Array<{ id: string; fullName: string }>;
};

export const FilterToolbar: FC<FilterToolbarProps> = ({
	selectedUnit,
	onUnitChange,
	selectedUser,
	onUserChange,
	users = [],
}) => {
	const { t } = useTranslation("home");

	const handleUnitChange = (event: React.ChangeEvent<HTMLSelectElement>): void => {
		event.preventDefault();
		const value = event.target.value;
		onUnitChange?.(value === "all" ? null : value);
	};

	const handleUserChange = (event: React.ChangeEvent<HTMLSelectElement>): void => {
		event.preventDefault();
		const value = event.target.value;
		if (value === "all") {
			onUserChange?.(null);
		} else if (value === "unassigned") {
			onUserChange?.("unassigned");
		} else {
			onUserChange?.(value);
		}
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
				<select
					className="px-3 py-2 border border-gray-300 rounded-lg text-sm bg-white"
					value={selectedUser ?? "all"}
					onChange={handleUserChange}
				>
					<option value="all">{t("filterToolbar.allUsers")}</option>
					<option value="unassigned">{t("table.unassigned")}</option>
					{users.map((user) => (
						<option key={user.id} value={user.id}>
							{user.fullName}
						</option>
					))}
				</select>
			</div>
		</div>
	);
};
