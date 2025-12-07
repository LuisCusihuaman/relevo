export type RecentPreview = {
	title: string;
	avatars: Array<{ src: string | null; fallback: string; bg: string }>;
	status: string;
	pr: string;
	color?: string;
};

export type SearchResult = {
	name: string;
	category: string;
	type: "handover" | "team" | "patient" | "assistant";
};

export type HandoverUI = {
	id: string;
	status: "Error" | "Ready";
	statusColor: string;
	environment: string;
	environmentColor: string;
	patientKey: string;
	patientName: string;
	patientIcon: {
		type: "text";
		value: string;
		bg: string;
		text?: string;
	};
	time: string;
	statusTime: string;
	environmentType: "Preview" | "Production";
	current?: boolean;
	bedLabel?: string;
	mrn?: string;
	author?: string;
	avatar?: string;
};

export type Metric = {
	label: string;
	value: string;
	tooltip: string;
	currentValue: string;
	totalValue: string;
};
