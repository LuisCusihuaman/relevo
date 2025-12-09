import { Button } from "@/components/ui/button";
import type { ShiftCheckInStep } from "../types";
import type { ReactElement } from "react";
import { useTranslation } from "react-i18next";

type MobileHeaderProps = {
	currentStep: ShiftCheckInStep;
	totalSteps: number;
	onSignOut: () => Promise<void>;
};

export function MobileHeader({
	currentStep,
	totalSteps,
	onSignOut,
}: MobileHeaderProps): ReactElement {
	const { t } = useTranslation(["dailySetup", "handover"]);
	const progressSteps = Array.from({ length: totalSteps }, (_, index) => index);

	return (
		<div
			className="flex-shrink-0 p-4 bg-white border-b border-border"
			style={{
				paddingTop: "max(env(safe-area-inset-top), 16px)",
			}}
		>
			<div className="flex items-center justify-between mb-4">
				<div>
					<h1 className="font-semibold text-primary">RELEVO</h1>
					<p className="text-xs text-muted-foreground">{t("mobileHeader.new")}</p>
				</div>
				<div className="flex items-center gap-3">
					<div className="text-sm text-muted-foreground">
						{t("mobileHeader.step", { current: currentStep + 1, total: totalSteps })}
					</div>
					<Button size="sm" variant="ghost" onClick={onSignOut}>
						{t("signOut", { ns: "dailySetup" }) || "Sign out"}
					</Button>
				</div>
			</div>

			<div className="flex gap-2">
				{progressSteps.map((step) => (
					<div
						key={step}
						className={`h-2 flex-1 rounded-full transition-colors ${
							step <= currentStep ? "bg-primary" : "bg-muted"
						}`}
					/>
				))}
			</div>
		</div>
	);
}
