import { useEffect, type FC } from "react";
import { useUser } from "@clerk/clerk-react";
import { useUserStore } from "@/store/user.store";

export const Home: FC = () => {
	const { user, isLoaded } = useUser();
	const { doctorName, setDoctorName } = useUserStore();

	useEffect(() => {
		if (!isLoaded) return;
		const fullName = user?.fullName ?? [user?.firstName, user?.lastName].filter(Boolean).join(" ");
		if (fullName && fullName !== doctorName) setDoctorName(fullName);
	}, [isLoaded, user, doctorName, setDoctorName]);

	return (
		<div className="p-4">
			<h1 className="text-2xl font-bold">
				{doctorName ? `Welcome back, ${doctorName}!` : "Hello, world!"}
			</h1>
		</div>
	);
};
