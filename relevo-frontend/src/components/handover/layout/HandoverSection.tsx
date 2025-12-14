import type { IpassGuidelineSection } from "@/common/constants";
import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from "@/components/ui/collapsible";
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from "@/components/ui/tooltip";
import { ChevronDown, ChevronUp, Info } from "lucide-react";
import type { JSX, ReactNode } from "react";

interface HandoverSectionProps {
  children: ReactNode;
  description: string;
  guidelines: IpassGuidelineSection;
  isMobile: boolean;
  isExpanded: boolean;
  letter: string;
  letterColor?: "blue" | "purple";
  onOpenChange?: (open: boolean) => void;
  title: string;
}

export function HandoverSection({
  children,
  description,
  guidelines,
  isMobile,
  isExpanded,
  letter,
  letterColor = "blue",
  onOpenChange,
  title,
}: HandoverSectionProps): JSX.Element {
  const colorClasses = {
    blue: "bg-blue-100 text-blue-700",
    purple: "bg-purple-100 text-purple-700",
  };

  const GuidelinesTooltip = (): JSX.Element => (
    <Tooltip>
      <TooltipTrigger asChild>
        <button 
          className="w-4 h-4 flex items-center justify-center opacity-60 hover:opacity-100 transition-opacity"
          onClick={(e) => { e.stopPropagation(); }}
        >
          <Info className="w-3 h-3 text-gray-400" />
        </button>
      </TooltipTrigger>
      <TooltipContent
        className="bg-white border border-gray-200 shadow-lg p-4 max-w-sm"
        side="top"
      >
        <div className="space-y-2">
          <h4 className="font-medium text-gray-900 text-sm">
            {guidelines.title}
          </h4>
          <ul className="space-y-1 text-xs text-gray-600">
            {guidelines.points.map((point, index) => (
              <li key={index} className="flex items-start space-x-1">
                <span className="text-gray-400 mt-0.5">•</span>
                <span>{point}</span>
              </li>
            ))}
          </ul>
        </div>
      </TooltipContent>
    </Tooltip>
  );

  const LetterBadge = (): JSX.Element => (
    <div className={`w-8 h-8 rounded-full flex items-center justify-center ${colorClasses[letterColor]}`}>
      <span className="font-bold">{letter}</span>
    </div>
  );

  // Single tree: Collapsible always mounted
  // Desktop: open=true, onOpenChange=undefined (no interaction)
  // Mobile: controlled by isExpanded + onOpenChange
  return (
    <Collapsible
      open={isMobile ? isExpanded : true}
      onOpenChange={isMobile ? onOpenChange : undefined}
    >
      <div className="bg-white rounded-lg border border-gray-100 overflow-hidden">
        <CollapsibleTrigger disabled={!isMobile} asChild>
          {/* All styling on the div child, CollapsibleTrigger stays minimal */}
          <div 
            className={`
              w-full p-4 border-b border-gray-100 transition-colors
              ${isMobile ? "cursor-pointer hover:bg-gray-50" : "pointer-events-none cursor-default"}
            `}
          >
            <div className="flex items-center justify-between">
              <div className="flex items-center space-x-4">
                <LetterBadge />
                <div>
                  <div className="flex items-center space-x-2">
                    <h3 className="font-semibold text-gray-900">{title}</h3>
                    <GuidelinesTooltip />
                  </div>
                  <p className="text-sm text-gray-700">{description}</p>
                </div>
              </div>
              {/* Chevron only visible on mobile */}
              {isMobile && (
                <div className="flex items-center">
                  {isExpanded ? (
                    <ChevronUp className="w-4 h-4 text-gray-500" />
                  ) : (
                    <ChevronDown className="w-4 h-4 text-gray-500" />
                  )}
                </div>
              )}
            </div>
          </div>
        </CollapsibleTrigger>
        <CollapsibleContent forceMount className="data-[state=closed]:hidden">
          {/* 
            forceMount + data-[state=closed]:hidden:
            Keeps content mounted (preserves editor state, focus) but hides via CSS when closed
          */}
          <div className="p-6">{children}</div>
        </CollapsibleContent>
      </div>
    </Collapsible>
  );
}
