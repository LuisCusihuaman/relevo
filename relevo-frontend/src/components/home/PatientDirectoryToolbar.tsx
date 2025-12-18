import type { FC } from "react";
import { useTranslation } from "react-i18next";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
	DropdownMenu,
	DropdownMenuContent,
	DropdownMenuItem,
	DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import {
	Search,
	Filter,
	Grid3X3,
	List,
	LucideChevronDown as DropdownMenuChevronDown,
} from "lucide-react";

type PatientDirectoryToolbarProps = {
	searchTerm?: string;
	onSearchChange?: (value: string) => void;
};

export const PatientDirectoryToolbar: FC<PatientDirectoryToolbarProps> = ({
	searchTerm = "",
	onSearchChange,
}) => {
	const { t } = useTranslation("home");
	return (
		<div className="flex items-center justify-between gap-4 mb-8">
			<div className="relative flex-1">
				<Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-gray-400" />
				<Input
					className="pl-10 h-10 border-gray-300 focus:border-gray-400 focus:ring-0 bg-white"
					placeholder={t("filterToolbar.searchPlaceholder")}
					value={searchTerm}
					onChange={(e) => {
						onSearchChange?.(e.target.value);
					}}
				/>
			</div>
			<div className="flex items-center gap-2">
				<div className="flex items-center gap-1">
					<Button
						className="h-10 w-10 p-0 border-gray-300 bg-white hover:bg-gray-50"
						size="sm"
						title={t("filterToolbar.filters")}
						variant="outline"
					>
						<Filter className="h-4 w-4" />
					</Button>
					<Button
						className="h-10 w-10 p-0 border-gray-300 bg-white hover:bg-gray-50"
						size="sm"
						title={t("filterToolbar.view")}
						variant="outline"
					>
						<Grid3X3 className="h-4 w-4" />
					</Button>
					<Button
						className="h-10 w-10 p-0 border-gray-300 bg-white hover:bg-gray-50"
						size="sm"
						title={t("filterToolbar.sort")}
						variant="outline"
					>
						<List className="h-4 w-4" />
					</Button>
				</div>
				<DropdownMenu>
					<DropdownMenuTrigger asChild>
						<Button className="bg-black text-white hover:bg-gray-800 h-10 px-4 ml-2">
							{t("filterToolbar.quickAction")}
							<DropdownMenuChevronDown className="ml-2 h-4 w-4" />
						</Button>
					</DropdownMenuTrigger>
					<DropdownMenuContent>
						<DropdownMenuItem>{t("filterToolbar.startHandover")}</DropdownMenuItem>
						<DropdownMenuItem>{t("filterToolbar.addAction")}</DropdownMenuItem>
						<DropdownMenuItem>{t("filterToolbar.markAlert")}</DropdownMenuItem>
						<DropdownMenuItem>{t("filterToolbar.inviteCollaborator")}</DropdownMenuItem>
					</DropdownMenuContent>
				</DropdownMenu>
			</div>
		</div>
	);
};
