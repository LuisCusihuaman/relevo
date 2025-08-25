import type { FC } from "react";
import { useUserStore } from "@/store/user.store";

export const Home: FC = () => {
	const { doctorName } = useUserStore(); // Concise-FP: use destructuring

	return (
		<div className="p-4">
			<h1 className="text-2xl font-bold">
				{doctorName ? `Welcome back, ${doctorName}!` : "Hello, world!"}
			</h1>
		</div>
	);
};
