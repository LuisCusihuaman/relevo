import { Button } from "@/components/ui/button";
import { ChevronLeft, ChevronRight } from "lucide-react";
import type { ShiftCheckInStep } from "../types";
import type { ReactElement } from "react";
import { useTranslation } from "react-i18next";

type ShiftCheckInNavigationProps = {
	currentStep: ShiftCheckInStep;
	totalSteps: number;
	canProceed: boolean;
	onBack: () => void;
	onNext: () => void;
};

export function ShiftCheckInNavigation({
	currentStep,
	totalSteps,
	canProceed,
	onBack,
	onNext,
}: ShiftCheckInNavigationProps): ReactElement {
	const { t } = useTranslation(["dailySetup", "handover"]);

	return (
		<>
			{/* Mobile Navigation */}
			<div
				className="md:hidden fixed top-0 left-0 right-0 z-30 px-4 py-3"
				style={{
					paddingTop: "max(env(safe-area-inset-top), 8px)",
				}}
			>
				<div className="rounded-xl border border-border/20 px-3 py-2 bg-background/95 backdrop-blur-sm">
					<div className="flex items-center gap-2">
						{currentStep > 0 && (
							<Button
								className="gap-2 h-10 px-4 rounded-lg"
								size="sm"
								variant="outline"
								onClick={onBack}
							>
								<ChevronLeft className="w-4 h-4" />
								{t("back")}
							</Button>
						)}

						<Button
							className={`flex-1 gap-2 h-10 rounded-lg ${canProceed ? 'cursor-pointer' : 'cursor-default'}`}
							disabled={!canProceed}
							size="sm"
							onClick={onNext}
						>
							{currentStep === totalSteps - 1
								? t("startUsingRelevo")
								: t("continue")}
							<ChevronRight className="w-4 h-4" />
						</Button>
					</div>
				</div>
			</div>

			{/* Desktop Navigation */}
			<div
				className="hidden md:block fixed top-0 left-0 right-0 z-30 px-6 py-3 bg-background/95 backdrop-blur-sm"
				style={{
					paddingTop: "max(env(safe-area-inset-top), 8px)",
				}}
			>
				<div className="border-b border-border/20 px-4 py-2">
					<div className="flex items-center justify-between">
						{currentStep > 0 ? (
							<Button className="gap-2" size="sm" variant="outline" onClick={onBack}>
								<ChevronLeft className="w-4 h-4" />
								{t("back")}
							</Button>
						) : (
							<div />
						)}

						<Button
							className={`gap-2 ${canProceed ? 'cursor-pointer' : 'cursor-default'}`}
							disabled={!canProceed}
							size="sm"
							onClick={onNext}
						>
							{currentStep === totalSteps - 1
								? t("completeSetup")
								: t("continue")}
							<ChevronRight className="w-4 h-4" />
						</Button>
					</div>
				</div>
			</div>
		</>
	);
}
