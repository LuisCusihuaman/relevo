import type { JSX } from "react";
import { useIsMobile } from "@/hooks/use-mobile";
import { useHandoverUIStore } from "@/store/handover-ui.store";
import { HistorySheet } from "./mobile/HistorySheet";
import { CollaborationSheet } from "./mobile/CollaborationSheet";
import { MobileMenuSheet } from "./mobile/MobileMenuSheet";

// No props needed!
export function MobileMenus(): JSX.Element {
  const isMobile = useIsMobile();
  
  // Optimized Zustand selectors - only needed for conditional rendering here
  // But sheets handle their own open state internally now via store
  const showHistory = useHandoverUIStore(state => state.showHistory);
  const showComments = useHandoverUIStore(state => state.showComments);
  const fullscreenEditing = useHandoverUIStore(state => state.fullscreenEditing);

  // Sheets are always mounted but control their own visibility
  // This allows them to animate properly when opening/closing
  
  return (
    <>
      {/* Mobile Menu Sheet */}
      <MobileMenuSheet />

      {/* Mobile History Sheet - Only show if valid conditions met */}
      {!isMobile && !fullscreenEditing && showHistory && (
        <HistorySheet />
      )}

      {/* Mobile Collaboration Sheet */}
      {!isMobile && !fullscreenEditing && showComments && (
        <CollaborationSheet />
      )}
    </>
  );
}
