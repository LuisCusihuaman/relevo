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
		<CardHeader className="text-center space-y-4">
			<div className="flex items-center justify-center gap-4">
				<div className="w-12 h-12 bg-white border border-border rounded-xl flex items-center justify-center shadow-sm">
					<div className="w-8 h-8 bg-primary rounded-lg flex items-center justify-center">
						<span className="text-primary-foreground font-semibold">R</span>
					</div>
				</div>
				<div>
					<CardTitle className="text-2xl text-foreground">{t("desktopHeader.new")}</CardTitle>
					<p className="text-muted-foreground">
						{currentStep === 0
							? t("desktopSubheader.new")
							: t("desktopSubheader.progress", {
									action: t("configure"),
									name: doctorName,
								})}
					</p>
				</div>
			</div>

			<div className="flex items-center justify-between">
				<Badge className="text-primary" variant="outline">
					{getStepTitle(currentStep)}
				</Badge>
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
		</CardHeader>
	);
}
