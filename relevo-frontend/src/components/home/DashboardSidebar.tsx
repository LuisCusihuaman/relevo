import type { FC } from "react";
import type { RecentPreview } from "@/components/home/types";
import { UsageCard } from "@/components/home/UsageCard";
import { RecentPreviewsCard } from "@/components/home/RecentPreviewsCard";

type DashboardSidebarProps = {
	recentPreviews: Array<RecentPreview>;
};

export const DashboardSidebar: FC<DashboardSidebarProps> = ({
	recentPreviews,
}) => {
	return (
		<div className="lg:w-96 space-y-6">
			<UsageCard />
			<RecentPreviewsCard recentPreviews={recentPreviews} />
		</div>
	);
};
