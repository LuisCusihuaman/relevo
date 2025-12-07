import type { FC } from "react";
import type { RecentPreview } from "@/types/domain";
import { SectionMeta } from "@/components/home/SectionMeta";
import { RecentActivityCard } from "@/components/home/RecentActivityCard";

type DashboardSidebarProps = {
	recentPreviews: Array<RecentPreview>;
};

export const DashboardSidebar: FC<DashboardSidebarProps> = ({
	recentPreviews,
}) => {
	return (
		<div className="lg:w-96 space-y-6">
			<SectionMeta />
			<RecentActivityCard recentPreviews={recentPreviews} />
		</div>
	);
};
