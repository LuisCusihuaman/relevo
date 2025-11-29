import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { CardHeader, CardTitle } from "@/components/ui/card";
import type { ShiftCheckInStep } from "../types";
import type { ReactElement } from "react";

type ShiftCheckInHeaderProps = {
	isMobile: boolean;
	currentStep: ShiftCheckInStep;
	doctorName: string;
	isEditing: boolean;
	totalSteps: number;
	onSignOut: () => Promise<void>;
	translation: (key: string, options?: Record<string, unknown>) => string;
	getStepTitle: (step: ShiftCheckInStep) => string;
};

export function ShiftCheckInHeader({
	isMobile,
	currentStep,
	doctorName,
	isEditing,
	totalSteps,
	onSignOut,
	translation: t,
	getStepTitle,
}: ShiftCheckInHeaderProps): ReactElement {
	const progressSteps = Array.from({ length: totalSteps }, (_, index) => index);

	if (isMobile) {
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
						<p className="text-xs text-muted-foreground">
							{isEditing ? t("mobileHeader.update") : t("mobileHeader.new")}
						</p>
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

	return (
		<CardHeader className="text-center space-y-4">
			<div className="flex items-center justify-center gap-4">
				<div className="w-12 h-12 bg-white border border-border rounded-xl flex items-center justify-center shadow-sm">
					<div className="w-8 h-8 bg-primary rounded-lg flex items-center justify-center">
						<span className="text-primary-foreground font-semibold">R</span>
					</div>
				</div>
				<div>
					<CardTitle className="text-2xl text-foreground">
						{isEditing ? t("desktopHeader.update") : t("desktopHeader.new")}
					</CardTitle>
					<p className="text-muted-foreground">
						{currentStep === 0
							? isEditing
								? t("desktopSubheader.update")
								: t("desktopSubheader.new")
							: t("desktopSubheader.progress", {
									action: isEditing ? t("update") : t("configure"),
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
