import type { FC } from "react";
import { useTranslation } from "react-i18next";
import { Info } from "lucide-react";
import type { PatientSummaryCard } from "@/types/domain";

type VersionNoticeProps = {
	patients?: ReadonlyArray<PatientSummaryCard>;
};

export const VersionNotice: FC<VersionNoticeProps> = ({ patients = [] }) => {
	const { t } = useTranslation("home");
	const totalPatients = patients.length;

	if (totalPatients === 0) {
		return null;
	}

	return (
		<div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
			<div className="flex items-start gap-3">
				<Info className="h-5 w-5 text-blue-600 mt-0.5 flex-shrink-0" />
				<div className="flex-1">
					<p className="text-sm text-blue-800">
						<strong>{t("versionNotice.title", { count: totalPatients })}</strong>
					</p>
				</div>
			</div>
		</div>
	);
};
