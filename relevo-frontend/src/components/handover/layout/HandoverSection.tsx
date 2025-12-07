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
  collapsible?: boolean;
  description: string;
  guidelines: IpassGuidelineSection;
  isExpanded?: boolean;
  letter: string;
  letterColor?: "blue" | "purple";
  title: string;
}

export function HandoverSection({
  children,
  collapsible = false,
  description,
  guidelines,
  isExpanded = true,
  letter,
  letterColor = "blue",
  title,
}: HandoverSectionProps): JSX.Element {
  const colorClasses = {
    blue: "bg-blue-100 text-blue-700",
    purple: "bg-purple-100 text-purple-700",
  };

  const GuidelinesTooltip = () => (
    <Tooltip>
      <TooltipTrigger asChild>
        <button className="w-4 h-4 flex items-center justify-center opacity-60 hover:opacity-100 transition-opacity">
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

  const LetterBadge = () => (
    <div className={`w-8 h-8 rounded-full flex items-center justify-center ${colorClasses[letterColor]}`}>
      <span className="font-bold">{letter}</span>
    </div>
  );

  // Collapsible mode (mobile)
  if (collapsible) {
    return (
      <Collapsible defaultOpen={isExpanded}>
        <div className="bg-white rounded-lg border border-gray-100 overflow-hidden">
          <CollapsibleTrigger asChild>
            <div className="p-4 bg-white border-b border-gray-100 cursor-pointer hover:bg-gray-50 transition-colors">
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
                <div className="flex items-center">
                  {isExpanded ? (
                    <ChevronUp className="w-4 h-4 text-gray-500" />
                  ) : (
                    <ChevronDown className="w-4 h-4 text-gray-500" />
                  )}
                </div>
              </div>
            </div>
          </CollapsibleTrigger>
          <CollapsibleContent>
            <div className="p-6">{children}</div>
          </CollapsibleContent>
        </div>
      </Collapsible>
    );
  }

  // Fixed mode (desktop)
  return (
    <div className="bg-white rounded-lg border border-gray-100">
      <div className="p-4 border-b border-gray-100">
        <div className="flex items-center space-x-3">
          <LetterBadge />
          <div className="flex-1">
            <h3 className="font-medium text-gray-900">{title}</h3>
            <p className="text-sm text-gray-600">{description}</p>
          </div>
          <GuidelinesTooltip />
        </div>
      </div>
      <div className="p-6">{children}</div>
    </div>
  );
}

