import { Button } from "@/components/ui/button";
import { ChevronLeft, ChevronRight } from "lucide-react";
import type { SetupStep } from "../types";
import type { ReactElement } from "react";

type SetupNavigationProps = {
	currentStep: SetupStep;
	totalSteps: number;
	isMobile: boolean;
	isEditing: boolean;
	canProceed: boolean;
	onBack: () => void;
	onNext: () => void;
	translation: (key: string, options?: Record<string, unknown>) => string;
};

export function SetupNavigation({
	currentStep,
	totalSteps,
	isMobile,
	isEditing,
	canProceed,
	onBack,
	onNext,
	translation: t,
}: SetupNavigationProps): ReactElement {
	if (isMobile) {
		return (
			<div
				className="fixed bottom-0 left-0 right-0 z-30"
				style={{
					paddingBottom: "max(env(safe-area-inset-bottom), 12px)",
				}}
			>
				<div className="bg-background/95 backdrop-blur-md mx-3 mb-3 rounded-2xl px-3 py-4 border border-border/40 shadow-lg">
					<div className="flex items-center gap-3">
						{currentStep > 0 && (
							<Button
								className="gap-2 h-12 px-6 rounded-xl"
								size="lg"
								variant="outline"
								onClick={onBack}
							>
								<ChevronLeft className="w-4 h-4" />
								{t("back")}
							</Button>
						)}

						<Button
							className="flex-1 gap-2 h-12 rounded-xl"
							disabled={!canProceed}
							size="lg"
							onClick={onNext}
						>
							{currentStep === totalSteps - 1
								? isEditing
									? t("saveChanges")
									: t("startUsingRelevo")
								: t("continue")}
							<ChevronRight className="w-4 h-4" />
						</Button>
					</div>
				</div>
			</div>
		);
	}

	return (
		<div className="flex items-center justify-between pt-6">
			{currentStep > 0 ? (
				<Button className="gap-2" variant="outline" onClick={onBack}>
					<ChevronLeft className="w-4 h-4" />
					{t("back")}
				</Button>
			) : (
				<div />
			)}

			<Button className="gap-2" disabled={!canProceed} onClick={onNext}>
				{currentStep === totalSteps - 1
					? isEditing
						? t("saveChanges")
						: t("completeSetup")
					: t("continue")}
				<ChevronRight className="w-4 h-4" />
			</Button>
		</div>
	);
}
