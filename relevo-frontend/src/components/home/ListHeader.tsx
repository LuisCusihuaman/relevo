import type { FC } from "react";
import { useTranslation } from "react-i18next";

export const ListHeader: FC = () => {
	const { t } = useTranslation("home");
	return (
		<div className="flex items-center justify-between">
			<div>
				<h1 className="text-2xl font-semibold text-gray-900">{t("listHeader.title")}</h1>
				<p className="text-sm text-gray-600 mt-1">{t("listHeader.subtitle")}</p>
			</div>
		</div>
	);
};
