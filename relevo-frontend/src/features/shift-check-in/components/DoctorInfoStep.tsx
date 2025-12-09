import { useTranslation } from "react-i18next";
import { Label } from "@/components/ui/label";
import { User } from "lucide-react";
import type { ReactElement } from "react";

type DoctorInfoStepProps = {
	doctorName: string;
};

export function DoctorInfoStep({
	doctorName,
}: DoctorInfoStepProps): ReactElement {
	const { t } = useTranslation(["dailySetup", "handover"]);

	return (
		<div className="space-y-6">
			<div className="text-center space-y-4">
				<div className="w-16 h-16 bg-white border border-border rounded-xl flex items-center justify-center mx-auto shadow-sm">
					<div className="w-10 h-10 bg-primary rounded-lg flex items-center justify-center">
						<span className="text-primary-foreground font-semibold text-lg">R</span>
					</div>
				</div>

				<div className="space-y-2">
					<h1 className="text-2xl font-semibold text-foreground">
						{t("welcome")}
					</h1>
					<p className="text-muted-foreground">
						{t("platformDescription")}
					</p>
					<div className="flex items-center justify-center gap-4 text-sm text-muted-foreground pt-1">
						<span>{t("ipassProtocol")}</span>
						<span>•</span>
						<span>{t("secureDocumentation")}</span>
					</div>
				</div>
			</div>

			<div className="space-y-4">
				<div className="space-y-2">
					<Label className="text-base font-medium flex items-center gap-2" htmlFor="doctorName">
						<User className="w-4 h-4 text-primary" />
						{t("yourNameLabel")}
					</Label>
					<div
						className="h-12 flex items-center rounded-md border border-border bg-muted/30 px-3 text-base text-foreground"
						id="doctorName"
					>
						<span className="truncate">{doctorName || "…"}</span>
					</div>
					<p className="text-sm text-muted-foreground">
						{t("nameHelp")}
					</p>
				</div>
			</div>
		</div>
	);
}
