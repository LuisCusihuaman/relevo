import type { FC } from "react";
import { useTranslation } from "react-i18next";
import { Calendar } from "lucide-react";

export const FilterToolbar: FC = () => {
	const { t } = useTranslation("home");
	return (
		<div className="flex items-center justify-between mt-6 mb-4">
			<div className="flex items-center gap-4">
				<button className="flex items-center gap-2 px-3 py-2 border border-gray-300 rounded-lg text-sm hover:bg-gray-50">
					<Calendar className="h-4 w-4" />
					{t("filterToolbar.dateRange")}
				</button>
			</div>
			<div className="flex items-center gap-4">
				<select className="px-3 py-2 border border-gray-300 rounded-lg text-sm bg-white">
					<option>{t("filterToolbar.allUnits")}</option>
					<option>{t("filterToolbar.icu")}</option>
					<option>{t("filterToolbar.emergency")}</option>
					<option>{t("filterToolbar.pediatrics")}</option>
					<option>{t("filterToolbar.cardiology")}</option>
				</select>
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
