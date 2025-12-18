import type { FC } from "react";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Button } from "@/components/ui/button";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import { LogOut } from "lucide-react";
import { useTranslation } from "react-i18next";
import { useUser } from "@clerk/clerk-react";
import { useSignOut } from "@/hooks/useSignOut";

type UserMenuPopoverProps = {
  onOpenMobileMenu?: () => void;
};

export const UserMenuPopover: FC<UserMenuPopoverProps> = ({ onOpenMobileMenu }) => {
  const { t } = useTranslation("home");
  const { user: clerkUser } = useUser();
  const { signOut } = useSignOut();

  const displayName = clerkUser?.fullName || "";
  const primaryEmail = clerkUser?.primaryEmailAddress?.emailAddress || clerkUser?.emailAddresses?.[0]?.emailAddress || "";

  return (
    <>
      {/* Hamburger Menu Button for Mobile */}
      <Button
        className="h-8 w-8 p-0 text-gray-600 hover:text-gray-900 md:hidden"
        size="sm"
        variant="ghost"
        onClick={() => {
          if (typeof onOpenMobileMenu === "function") onOpenMobileMenu();
          else window.dispatchEvent(new CustomEvent("open-mobile-menu"));
        }}
      >
        <svg className="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path d="M4 6h16M4 12h16M4 18h16" strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} />
        </svg>
      </Button>

      <Popover>
      <PopoverTrigger asChild>
        <button className="flex items-center gap-2 px-3 py-1.5 rounded-md hover:bg-gray-100 transition-colors cursor-pointer hidden md:flex">
          <Avatar className="h-8 w-8">
            <AvatarImage src={clerkUser?.imageUrl} />
            <AvatarFallback>{clerkUser?.firstName?.[0] || displayName?.[0] || "U"}</AvatarFallback>
          </Avatar>
          <span className="text-sm font-medium text-gray-700">{displayName || "Usuario"}</span>
        </button>
      </PopoverTrigger>
      <PopoverContent align="end" className="w-80 p-0 z-50">
        <div className="p-4 border-b border-gray-100">
          <div className="font-medium text-gray-900">{displayName}</div>
          <div className="text-sm text-gray-600">{primaryEmail}</div>
        </div>
        <div className="p-2">
          <button
            className="flex items-center justify-between px-3 py-2 text-gray-700 w-full text-left rounded-md focus:outline-none"
            onClick={() => signOut()}
          >
            <div className="flex items-center gap-3">
              <LogOut className="h-4 w-4" />
              {t("userMenu.logOut")}
            </div>
            <LogOut className="h-4 w-4 text-gray-400" />
          </button>
        </div>
      </PopoverContent>
      </Popover>
    </>
  );
};

export default UserMenuPopover;


