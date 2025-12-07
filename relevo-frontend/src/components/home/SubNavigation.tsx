import type { FC } from "react";
import { useTranslation } from "react-i18next";
import { Link, useLocation } from "@tanstack/react-router";

export const SubNavigation: FC = () => {
	const { t } = useTranslation("home");
	const location = useLocation();

	const tabs: Array<{ key: string; label: string; path: string }> = [
		{ key: "summary", label: t("subnav.summary"), path: "/dashboard" },
		{
			key: "patients",
			label: t("subnav.patients"),
			path: "/patients",
		},
		{
			key: "settings",
			label: t("subnav.settings"),
			path: "/settings",
		},
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
						<Link
							key={tab.key}
							to={tab.path}
							className={`text-sm transition-colors relative py-3 whitespace-nowrap flex-shrink-0 ${
								location.pathname === tab.path
									? "text-gray-900 font-medium"
									: "text-gray-600 hover:text-gray-900 font-normal"
							}`}
						>
							{tab.label}
							{location.pathname === tab.path && (
								<div className="absolute bottom-0 left-0 right-0 h-0.5 bg-black" />
							)}
						</Link>
					))}
				</nav>
			</div>
		</div>
	);
};
