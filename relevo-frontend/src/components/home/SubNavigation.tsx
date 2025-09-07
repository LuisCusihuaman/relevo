import type { FC } from "react";
import { useTranslation } from "react-i18next";

type SubNavigationProps = {
	activeTab: string;
	setActiveTab: (tab: string) => void;
};

export const SubNavigation: FC<SubNavigationProps> = ({
	activeTab,
	setActiveTab,
}) => {
	const { t } = useTranslation("home");
	const tabs: Array<{ key: string; label: string }> = [
		{ key: "summary", label: t("subnav.summary") as string },
		{ key: "patients", label: t("subnav.patients") as string },
		{ key: "settings", label: t("subnav.settings") as string },
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
					{tabs.map((tab) => (
						<button
							key={tab.key}
							className={`text-sm transition-colors relative py-3 whitespace-nowrap flex-shrink-0 ${
								activeTab === tab.key
									? "text-gray-900 font-medium"
									: "text-gray-600 hover:text-gray-900 font-normal"
							}`}
							onClick={() => {
								setActiveTab(tab.key);
							}}
						>
							{tab.label}
							{activeTab === tab.key && (
								<div className="absolute bottom-0 left-0 right-0 h-0.5 bg-black" />
							)}
						</button>
					))}
				</nav>
			</div>
		</div>
	);
};
