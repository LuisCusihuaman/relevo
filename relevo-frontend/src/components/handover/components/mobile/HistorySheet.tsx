import type { JSX } from "react";
import {
  Sheet,
  SheetContent,
  SheetDescription,
  SheetHeader,
  SheetTitle,
} from "@/components/ui/sheet";
import { HandoverHistory } from "../HandoverHistory";
import { useTranslation } from "react-i18next";
import { useHandoverUIStore } from "@/store/handover-ui.store";
import { useCurrentHandover } from "../../hooks/useCurrentHandover";

export function HistorySheet(): JSX.Element | null {
  const { t } = useTranslation("mobileMenus");
  
  // Store
  const showHistory = useHandoverUIStore(state => state.showHistory);
  const setShowHistory = useHandoverUIStore(state => state.setShowHistory);
  const fullscreenEditing = useHandoverUIStore(state => state.fullscreenEditing);

  // Data
  const { handoverId, patientData } = useCurrentHandover();

  // Don't render if not open or if in fullscreen
  if (!showHistory || fullscreenEditing) return null;

  const patientInfo = patientData ? {
    name: patientData.name,
    mrn: patientData.mrn,
    admissionDate: patientData.admissionDate || ""
  } : { name: "", mrn: "", admissionDate: "" };

  return (
    <Sheet open={showHistory} onOpenChange={setShowHistory}>
      <SheetContent
        className="w-80 bg-white border-r border-gray-200 p-0"
        side="left"
      >
        <SheetHeader className="p-4 border-b border-gray-200">
          <SheetTitle className="text-left text-gray-900">
            {t("historySheet.title")}
          </SheetTitle>
          <SheetDescription className="text-left text-gray-600">
            {t("historySheet.description", { patientName: patientData?.name || "Patient" })}
          </SheetDescription>
        </SheetHeader>
        <div className="flex-1 overflow-auto">
          <HandoverHistory
            hideHeader
            handoverId={handoverId || ""}
            patientData={patientInfo}
            onClose={() => { setShowHistory(false); }}
          />
        </div>
      </SheetContent>
    </Sheet>
  );
}
