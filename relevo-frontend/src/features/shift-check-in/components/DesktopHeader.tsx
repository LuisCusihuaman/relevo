import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { CardHeader, CardTitle } from "@/components/ui/card";
import type { ShiftCheckInStep } from "../types";
import type { ReactElement } from "react";
import { useTranslation } from "react-i18next";

type DesktopHeaderProps = {
	currentStep: ShiftCheckInStep;
	doctorName: string;
	totalSteps: number;
	onSignOut: () => Promise<void>;
	getStepTitle: (step: ShiftCheckInStep) => string;
};

export function DesktopHeader({
	currentStep,
	doctorName,
	totalSteps,
	onSignOut,
	getStepTitle,
}: DesktopHeaderProps): ReactElement {
	const { t } = useTranslation(["dailySetup", "handover"]);
	const progressSteps = Array.from({ length: totalSteps }, (_, index) => index);

	return (
		<CardHeader className="text-center space-y-3 pb-4 pt-20">
			<div className="flex items-center justify-center gap-3">
				<div className="w-10 h-10 bg-white border border-border rounded-xl flex items-center justify-center shadow-sm flex-shrink-0">
					<div className="w-7 h-7 bg-primary rounded-lg flex items-center justify-center">
						<span className="text-primary-foreground font-semibold text-sm">R</span>
					</div>
				</div>
				<div className="min-w-0 flex-1">
					<CardTitle className="text-xl text-foreground break-words">{t("desktopHeader.new")}</CardTitle>
					<p className="text-sm text-muted-foreground break-words mt-1">
						{currentStep === 0
							? t("desktopSubheader.new")
							: t("desktopSubheader.progress", {
									action: t("configure"),
									name: doctorName,
								})}
					</p>
				</div>
			</div>

			<div className="flex items-center justify-between pt-2">
				<Badge className="text-primary text-xs" variant="outline">
					{getStepTitle(currentStep)}
				</Badge>
				<div className="flex items-center gap-3">
					<div className="text-xs text-muted-foreground">
						{t("mobileHeader.step", { current: currentStep + 1, total: totalSteps })}
					</div>
					<Button size="sm" variant="ghost" onClick={onSignOut}>
						{t("signOut", { ns: "dailySetup" }) || "Sign out"}
					</Button>
				</div>
			</div>

			<div className="flex gap-2 pt-1">
				{progressSteps.map((step) => (
					<div
						key={step}
						className={`h-1.5 flex-1 rounded-full transition-colors ${
							step <= currentStep ? "bg-primary" : "bg-muted"
						}`}
					/>
				))}
			</div>
		</CardHeader>
	);
}
