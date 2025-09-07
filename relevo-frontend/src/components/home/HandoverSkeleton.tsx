import type { FC } from "react";

export const HandoverSkeleton: FC = () => {
	const skeletonRows = Array.from({ length: 5 }, (_, index) => index);

	return (
		<>
			{/* Desktop Skeleton */}
			<div className="hidden md:block rounded-lg border border-gray-200 bg-white overflow-hidden">
				{skeletonRows.map((index) => (
					<div
						key={`desktop-${index}`}
						className={`grid grid-cols-[1fr_1fr_1fr_1fr_1fr] items-center gap-4 py-3 px-4 ${
							index < 4 ? "border-b border-gray-100" : ""
						}`}
					>
						{/* Left Column: Location/MRN */}
						<div className="min-w-0 space-y-2">
							<div className="h-4 bg-gray-200 rounded animate-pulse w-3/4"></div>
							<div className="h-3 bg-gray-100 rounded animate-pulse w-1/2"></div>
						</div>

						{/* Status Column */}
						<div className="min-w-0 space-y-2">
							<div className="flex items-center gap-2">
								<div className="h-2 w-2 bg-gray-200 rounded-full animate-pulse"></div>
								<div className="h-4 bg-gray-200 rounded animate-pulse w-16"></div>
							</div>
							<div className="h-3 bg-gray-100 rounded animate-pulse w-12"></div>
						</div>

						{/* Patient/Source Column */}
						<div className="min-w-0 space-y-2">
							<div className="flex items-center gap-2">
								<div className="h-5 w-5 bg-gray-200 rounded animate-pulse"></div>
								<div className="h-4 bg-gray-200 rounded animate-pulse w-20"></div>
								<div className="h-6 w-6 bg-gray-200 rounded animate-pulse"></div>
							</div>
							<div className="flex items-center gap-1">
								<div className="h-3 w-3 bg-gray-200 rounded animate-pulse"></div>
								<div className="h-3 bg-gray-200 rounded animate-pulse w-24"></div>
							</div>
						</div>

						{/* Created Column */}
						<div className="min-w-0 text-right space-y-2">
							<div className="flex items-center justify-end gap-2">
								<div className="h-3 w-3 bg-gray-200 rounded animate-pulse"></div>
								<div className="h-3 bg-gray-200 rounded animate-pulse w-16"></div>
								<div className="h-6 w-6 bg-gray-200 rounded-full animate-pulse"></div>
							</div>
						</div>
					</div>
				))}
			</div>

			{/* Mobile Skeleton */}
			<div className="md:hidden space-y-4">
				{skeletonRows.map((index) => (
					<div
						key={`mobile-${index}`}
						className="bg-white border border-gray-200 rounded-lg p-4 space-y-3"
					>
						{/* Card Header */}
						<div className="flex items-center justify-between">
							<div className="space-y-1">
								<div className="h-5 bg-gray-200 rounded animate-pulse w-32"></div>
								<div className="h-4 bg-gray-100 rounded animate-pulse w-20"></div>
							</div>
							<div className="h-6 w-6 bg-gray-200 rounded animate-pulse"></div>
						</div>

						{/* Status */}
						<div className="flex items-center gap-2">
							<div className="h-2 w-2 bg-gray-200 rounded-full animate-pulse"></div>
							<div className="h-4 bg-gray-200 rounded animate-pulse w-16"></div>
							<div className="h-3 bg-gray-100 rounded animate-pulse w-12"></div>
						</div>

						{/* Environment */}
						<div className="space-y-1">
							<div className="h-4 bg-gray-200 rounded animate-pulse w-24"></div>
							<div className="h-3 bg-gray-100 rounded animate-pulse w-16"></div>
						</div>

						{/* Patient Info */}
						<div className="flex items-center gap-2">
							<div className="h-6 w-6 bg-gray-200 rounded animate-pulse"></div>
							<div className="h-4 bg-gray-200 rounded animate-pulse w-20"></div>
						</div>

						{/* Clinical Meta */}
						<div className="flex items-center gap-1">
							<div className="h-4 w-4 bg-gray-200 rounded animate-pulse"></div>
							<div className="h-3 bg-gray-200 rounded animate-pulse w-28"></div>
						</div>

						{/* Author */}
						<div className="flex items-center justify-between pt-2 border-t border-gray-100">
							<div className="flex items-center gap-2">
								<div className="h-6 w-6 bg-gray-200 rounded-full animate-pulse"></div>
								<div className="h-3 bg-gray-200 rounded animate-pulse w-20"></div>
							</div>
						</div>
					</div>
				))}
			</div>
		</>
	);
};
