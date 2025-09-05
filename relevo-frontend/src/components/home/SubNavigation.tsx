import type { FC } from "react";

type SubNavigationProps = {
	isProjectView: boolean;
	activeTab: string;
	setActiveTab: (tab: string) => void;
};

export const SubNavigation: FC<SubNavigationProps> = ({
	isProjectView,
	activeTab,
	setActiveTab,
}) => {
	const tabs = isProjectView
		? [
				"Overview",
				"Deployments",
				"Analytics",
				"Speed Insights",
				"Logs",
				"Observability",
				"Firewall",
				"Storage",
				"Flags",
				"Settings",
			]
		: [
				"Overview",
				"Integrations",
				"Deployments",
				"Activity",
				"Domains",
				"Usage",
				"Observability",
				"Storage",
				"Flags",
				"AI Gateway",
				"Support",
				"Settings",
			];

	return (
		<div className="sticky top-0 z-50 border-b border-gray-200 bg-white">
			<div className="px-4 md:px-6">
				<nav
					className="flex space-x-6 overflow-x-auto"
					style={{
						scrollbarWidth: "none",
						msOverflowStyle: "none",
					}}
				>
					{/* Tailwind-v4: Hide scrollbar cross-browser */}
					{/* Readable-JSX: Use Tailwind utilities instead of <style jsx> */}
					{/* 
                      - 'scrollbar-none' is a Tailwind v4 utility for hiding scrollbars.
                      - If not available, fallback to 'scrollbar-hide' from a plugin or custom CSS.
                  */}
					{tabs.map((tab) => (
						<button
							key={tab}
							className={`text-sm transition-colors relative py-3 whitespace-nowrap flex-shrink-0 ${
								activeTab === tab
									? "text-gray-900 font-medium"
									: "text-gray-600 hover:text-gray-900 font-normal"
							}`}
							onClick={() => {
								if (tab === "Deployments" && !isProjectView) {
									window.location.href = "/deployments";
								} else {
									setActiveTab(tab);
								}
							}}
						>
							{tab}
							{activeTab === tab && (
								<div className="absolute bottom-0 left-0 right-0 h-0.5 bg-black" />
							)}
						</button>
					))}
				</nav>
			</div>
		</div>
	);
};
