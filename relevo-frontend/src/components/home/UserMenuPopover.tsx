import type { FC } from "react";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Button } from "@/components/ui/button";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import { Command, HomeIcon, LogOut, Monitor, Plus, Settings, Sun, Moon, User } from "lucide-react";
import { useTranslation } from "react-i18next";
import { useUser, useClerk } from "@clerk/clerk-react";

type UserMenuPopoverProps = {
  onOpenMobileMenu?: () => void;
};

export const UserMenuPopover: FC<UserMenuPopoverProps> = ({ onOpenMobileMenu }) => {
  const { t } = useTranslation("home");
  const { user } = useUser();
  const { signOut } = useClerk();
  const displayName = user?.fullName ?? "";
  const primaryEmail = user?.primaryEmailAddress?.emailAddress ?? user?.emailAddresses?.[0]?.emailAddress ?? "";

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
        <Avatar className="h-8 w-8 cursor-pointer hidden md:block">
          <AvatarImage src="/placeholder.svg?height=32&width=32" />
          <AvatarFallback>LC</AvatarFallback>
        </Avatar>
      </PopoverTrigger>
      <PopoverContent align="end" className="w-80 p-0 z-50">
        <div className="p-4 border-b border-gray-100">
          <div className="font-medium text-gray-900">{displayName}</div>
          <div className="text-sm text-gray-600">{primaryEmail}</div>
        </div>
        <div className="p-2">
          <button className="flex items-center gap-3 px-3 py-2 text-gray-700 w-full text-left rounded-md focus:outline-none">
            <User className="h-4 w-4" />
            {t("userMenu.dashboard")}
          </button>
          <button className="flex items-center gap-3 px-3 py-2 text-gray-700 w-full text-left rounded-md focus:outline-none">
            <Settings className="h-4 w-4" />
            {t("userMenu.accountSettings")}
          </button>
          <button className="flex items-center justify-between px-3 py-2 text-gray-700 w-full text-left rounded-md focus:outline-none">
            <div className="flex items-center gap-3">
              <Plus className="h-4 w-4" />
              {t("userMenu.createTeam")}
            </div>
            <Plus className="h-4 w-4 text-gray-400" />
          </button>
          <button className="flex items-center justify-between px-3 py-2 text-gray-700 w-full text-left rounded-md focus:outline-none">
            <div className="flex items-center gap-3">
              <Command className="h-4 w-4" />
              {t("userMenu.commandMenu")}
            </div>
            <div className="flex items-center gap-1">
              <kbd className="text-xs text-gray-500 bg-gray-100 px-1.5 py-0.5 rounded">⌘</kbd>
              <kbd className="text-xs text-gray-500 bg-gray-100 px-1.5 py-0.5 rounded">K</kbd>
            </div>
          </button>
          <button className="flex items-center justify-between px-3 py-2 text-gray-700 w-full text-left rounded-md focus:outline-none">
            <div className="flex items-center gap-3">
              <Monitor className="h-4 w-4" />
              {t("userMenu.theme")}
            </div>
            <div className="flex items-center gap-1">
              <Monitor className="h-4 w-4 text-gray-400" />
              <Sun className="h-4 w-4 text-gray-400" />
              <Moon className="h-4 w-4 text-gray-400" />
            </div>
          </button>
          <button className="flex items-center justify-between px-3 py-2 text-gray-700 w-full text-left rounded-md focus:outline-none">
            <div className="flex items-center gap-3">
              <HomeIcon className="h-4 w-4" />
              {t("userMenu.homePage")}
            </div>
            <svg className="text-gray-400" fill="currentColor" height="16" viewBox="0 0 75 65" width="16">
              <path d="M37.59.25l36.95 64H.64l36.95-64z" />
            </svg>
          </button>
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
        <div className="p-4 border-t border-gray-100">
          <Button className="w-full bg-black text-white hover:bg-gray-800">{t("userMenu.upgradeToPro")}</Button>
        </div>
      </PopoverContent>
      </Popover>
    </>
  );
};

export default UserMenuPopover;


