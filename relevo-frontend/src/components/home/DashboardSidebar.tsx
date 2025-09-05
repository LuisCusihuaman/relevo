import { type FC } from "react";
import { type RecentPreview } from "./types";
import { UsageCard } from "./UsageCard";
import { RecentPreviewsCard } from "./RecentPreviewsCard";

type DashboardSidebarProps = {
	recentPreviews: RecentPreview[];
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
