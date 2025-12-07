import type { JSX } from "react";
import {
  Sheet,
  SheetContent,
  SheetDescription,
  SheetHeader,
  SheetTitle,
} from "@/components/ui/sheet";
import { CollaborationPanel } from "../CollaborationPanel";
import { useTranslation } from "react-i18next";
import { useHandoverUIStore } from "@/store/handover-ui.store";
import { useCurrentHandover } from "../../hooks/useCurrentHandover";

export function CollaborationSheet(): JSX.Element | null {
  const { t } = useTranslation("mobileMenus");
  
  // Store
  const showComments = useHandoverUIStore(state => state.showComments);
  const setShowComments = useHandoverUIStore(state => state.setShowComments);
  const fullscreenEditing = useHandoverUIStore(state => state.fullscreenEditing);

  // Data
  const { handoverId, patientData } = useCurrentHandover();

  // Don't render if not open or if in fullscreen
  if (!showComments || fullscreenEditing) return null;

  return (
    <Sheet open={showComments} onOpenChange={setShowComments}>
      <SheetContent
        className="w-80 bg-white border-l border-gray-200 p-0"
        side="right"
      >
        <SheetHeader className="p-4 border-b border-gray-200">
          <SheetTitle className="text-left text-gray-900">
            {t("collaborationSheet.title")}
          </SheetTitle>
          <SheetDescription className="text-left text-gray-600">
            {t("collaborationSheet.description", {
              patientName: patientData?.name || "Patient",
            })}
          </SheetDescription>
        </SheetHeader>
        <div className="flex-1 overflow-auto">
          <CollaborationPanel
            hideHeader
            handoverId={handoverId || ""}
            onClose={() => { setShowComments(false); }}
          />
        </div>
      </SheetContent>
    </Sheet>
  );
}
