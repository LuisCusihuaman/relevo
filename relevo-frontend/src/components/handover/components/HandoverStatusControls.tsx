import type { JSX } from "react";
import { Button } from "@/components/ui/button";
import {
	useReadyHandover,
	useStartHandover,
} from "@/api/endpoints/handovers";
import type { HandoverDetail } from "@/types/domain";
import { Loader2, CheckCircle2, Play, AlertCircle } from "lucide-react";
import { toast } from "sonner";
import { Badge } from "@/components/ui/badge";

interface HandoverStatusControlsProps {
	handover: HandoverDetail;
	isSender: boolean;
	isReceiver: boolean;
}

export function HandoverStatusControls({
	handover,
	isSender,
	isReceiver,
}: HandoverStatusControlsProps): JSX.Element | null {
	const { mutate: markReady, isPending: isReadyPending } = useReadyHandover();
	const { mutate: startHandover, isPending: isStartPending } = useStartHandover();

	const handleReady = (): void => {
		markReady(handover.id, {
			onSuccess: () => toast.success("Handover marked as Ready"),
			onError: (err) => toast.error(`Error: ${err.message}`),
		});
	};

	const handleStart = (): void => {
		startHandover(handover.id, {
			onSuccess: () => toast.success("Handover started"),
			onError: (err) => toast.error(`Error: ${err.message}`),
		});
	};

	// Status: Draft
	if (handover.stateName === "Draft") {
		if (!isSender) {
			return (
				<Badge variant="secondary" className="bg-gray-100 text-gray-600 border-gray-200">
					Draft • Waiting for Sender
				</Badge>
			);
		}
		return (
			<Button
				size="sm"
				onClick={handleReady}
				disabled={isReadyPending}
				className="bg-blue-600 hover:bg-blue-700 text-white cursor-pointer h-7 text-xs px-3 shadow-sm"
			>
				{isReadyPending ? (
					<Loader2 className="w-3 h-3 mr-1.5 animate-spin" />
				) : (
					<CheckCircle2 className="w-3 h-3 mr-1.5" />
				)}
				Mark as Ready
			</Button>
		);
	}

	// Status: Ready
	if (handover.stateName === "Ready") {
		if (!isReceiver) {
			return (
				<Badge variant="secondary" className="bg-blue-50 text-blue-700 border-blue-200 h-7">
					Ready • Waiting for Receiver
				</Badge>
			);
		}
		return (
			<div className="flex items-center gap-2">
				<Button
					size="sm"
					onClick={handleStart}
					disabled={isStartPending}
					className="bg-green-600 hover:bg-green-700 text-white cursor-pointer h-7 text-xs px-3 shadow-sm"
				>
					{isStartPending ? (
						<Loader2 className="w-3 h-3 mr-1.5 animate-spin" />
					) : (
						<Play className="w-3 h-3 mr-1.5" />
					)}
					Start Handover
				</Button>
			</div>
		);
	}

	// Status: InProgress - Receiver must complete via Synthesis section checklist
	if (handover.stateName === "InProgress") {
		if (isReceiver) {
			return (
				<Badge variant="outline" className="bg-purple-50 text-purple-700 border-purple-200 h-7 animate-pulse">
					In Progress • Verify Checklist Below
				</Badge>
			);
		}
		return (
			<Badge variant="secondary" className="bg-amber-50 text-amber-700 border-amber-200 animate-pulse h-7">
				In Progress
			</Badge>
		);
	}

	// Status: Completed
	if (handover.stateName === "Completed") {
		return (
			<Badge variant="secondary" className="bg-green-50 text-green-700 border-green-200">
				<CheckCircle2 className="w-3 h-3 mr-1" />
				Completed
			</Badge>
		);
	}

	// Status: Cancelled
	if (handover.stateName === "Cancelled") {
		return (
			<Badge variant="secondary" className="bg-red-50 text-red-700 border-red-200">
				<AlertCircle className="w-3 h-3 mr-1" />
				Cancelled
			</Badge>
		);
	}

	return null;
}
