import type { FC } from "react";
import type { RecentPreview, PatientSummaryCard } from "@/types/domain";
import { SectionMeta } from "@/components/home/SectionMeta";
import { RecentActivityCard } from "@/components/home/RecentActivityCard";

type DashboardSidebarProps = {
	recentPreviews: Array<RecentPreview>;
	patients: ReadonlyArray<PatientSummaryCard>;
};

export const DashboardSidebar: FC<DashboardSidebarProps> = ({
	recentPreviews,
	patients,
}) => {
	return (
		<div className="lg:w-96 space-y-6">
			<SectionMeta patients={patients} />
			<RecentActivityCard recentPreviews={recentPreviews} />
		</div>
	);
};
