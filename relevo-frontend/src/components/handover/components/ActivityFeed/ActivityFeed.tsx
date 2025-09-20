import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { ScrollArea } from "@/components/ui/scroll-area";
import { useTranslation } from "react-i18next";

export interface ActivityItem {
  id: number | string;
  user: string;
  userInitials: string;
  userColor: string;
  action: string;
  section: string;
  time: string;
  type: "user_joined" | "content_updated" | "content_added" | "content_viewed";
}

interface ActivityFeedProps {
  items: Array<ActivityItem>;
  onNavigateToSection: (section: string) => void;
}

export function ActivityFeed({ items, onNavigateToSection }: ActivityFeedProps): JSX.Element {
  const { t } = useTranslation("activityFeed");
  const renderActivityDot = (type: ActivityItem["type"]): JSX.Element => {
    switch (type) {
      case "content_updated":
        return <div className="w-2 h-2 bg-blue-500 rounded-full"></div>;
      case "content_added":
        return <div className="w-2 h-2 bg-green-500 rounded-full"></div>;
      case "user_joined":
        return <div className="w-2 h-2 bg-purple-500 rounded-full"></div>;
      case "content_viewed":
        return <div className="w-2 h-2 bg-gray-400 rounded-full"></div>;
      default:
        return null;
    }
  };

  return (
    <ScrollArea className="flex-1">
      <div className="p-4 space-y-3">
        {items.map((activity, index) => (
          <div key={activity.id}>
            <div className="flex items-start space-x-3">
              <Avatar className="w-7 h-7 flex-shrink-0">
                <AvatarFallback
                  className={`${activity.userColor} text-white text-xs`}
                >
                  {activity.userInitials}
                </AvatarFallback>
              </Avatar>
              <div className="flex-1 min-w-0">
                <div className="flex items-center space-x-2">
                  <span className="text-sm font-medium text-gray-900">
                    {activity.user}
                  </span>
                  <span className="text-xs text-gray-500">{activity.time}</span>
                </div>
                <div className="text-sm text-gray-600">
                  {activity.action}
                  {activity.section !== "General" && (
                    <button
                      className="text-blue-600 hover:text-blue-700 ml-1 hover:underline"
                      onClick={() =>
                        { onNavigateToSection(
                          activity.section.toLowerCase().replace(" ", ""),
                        ); }
                      }
                    >
                      {t("inSection", { section: activity.section })}
                    </button>
                  )}
                </div>
              </div>
              <div className="flex-shrink-0">
                {renderActivityDot(activity.type)}
              </div>
            </div>
            {index < items.length - 1 && (
              <div className="ml-10 mt-3 border-b border-gray-100"></div>
            )}
          </div>
        ))}
      </div>
    </ScrollArea>
  );
} 