import type { FC } from "react";

export const PatientDirectorySkeleton: FC = () => {
	const skeletonItems = Array.from({ length: 5 }, (_, index) => index);

	return (
		<div className="flex-1 min-w-0">
			<div className="mb-4">
				<div className="h-5 bg-gray-200 rounded animate-pulse w-48"></div>
			</div>
			<div className="bg-white border border-gray-200 rounded-xl shadow-sm overflow-hidden">
				<ul className="divide-y divide-gray-200">
					{skeletonItems.map((index) => (
						<li
							key={index}
							className="grid grid-cols-[minmax(0,2fr)_minmax(0,2fr)_minmax(0,1fr)] items-center gap-6 py-4 px-6"
						>
							{/* Patient info skeleton */}
							<div className="flex items-center gap-3 min-w-0">
								<div className="h-10 w-10 rounded-full bg-gray-200 animate-pulse shrink-0"></div>
								<div className="min-w-0 flex-1 space-y-2">
									<div className="h-4 bg-gray-200 rounded animate-pulse w-3/4"></div>
									<div className="h-3 bg-gray-100 rounded animate-pulse w-1/2"></div>
								</div>
							</div>

							{/* Action skeleton */}
							<div className="min-w-0 flex-1 space-y-2">
								<div className="h-4 bg-gray-200 rounded animate-pulse w-32"></div>
								<div className="flex items-center gap-2">
									<div className="h-3 bg-gray-100 rounded animate-pulse w-8"></div>
									<div className="h-3 w-3 bg-gray-200 rounded animate-pulse"></div>
									<div className="h-3 bg-gray-100 rounded animate-pulse w-16"></div>
								</div>
							</div>

							{/* Buttons skeleton */}
							<div className="flex items-center justify-end gap-2 shrink-0 min-w-0">
								<div className="h-8 w-8 bg-gray-200 rounded-full animate-pulse"></div>
								<div className="h-8 w-8 bg-gray-200 rounded-full animate-pulse"></div>
							</div>
						</li>
					))}
				</ul>
			</div>
		</div>
	);
};
