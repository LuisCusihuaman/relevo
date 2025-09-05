export type Project = {
	name: string;
	url: string;
	status: string;
	date: string;
	icon: string;
	hasGithub: boolean;
	branch?: string;
	github?: string;
};

export type RecentPreview = {
	title: string;
	avatars: { src: string | null; fallback: string; bg: string }[];
	status: string;
	pr: string;
	color?: string;
};

export type SearchResult = {
	name: string;
	category: string;
	type: "deployment" | "team" | "project" | "assistant";
};

export type Deployment = {
	id: string;
	status: "Error" | "Ready";
	statusColor: string;
	environment: string;
	environmentColor: string;
	project: string;
	projectIcon: {
		type: "emoji" | "text";
		value: string;
		bg: string;
		text?: string;
	};
	branch: string;
	commit: string;
	message: string;
	time: string;
	author: string;
	statusTime: string;
	environmentType: "Preview" | "Production";
	avatar: string;
	current?: boolean;
};
