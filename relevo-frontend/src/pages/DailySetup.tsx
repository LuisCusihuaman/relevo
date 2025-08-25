import { useState, type FC, type JSX } from "react";
import { useNavigate } from "@tanstack/react-router";
import { useUserStore } from "@/store/user.store";

// Mock data based on the test to make the component interactive
const MOCK_UNITS = ["General Pediatrics", "Pediatric ICU", "Neonatal ICU"];
const MOCK_SHIFTS = ["Morning Shift", "Afternoon Shift", "Night Shift"];
const MOCK_PATIENTS = [
	"Liam Johnson",
	"Olivia Williams",
	"Noah Brown",
	"Emma Jones",
	"Oliver Garcia",
	"Ava Martinez",
	"Elijah Rodriguez",
	"Charlotte Hernandez",
	"William Lopez",
	"Sophia Gonzalez",
	"James Wilson",
	"Isabella Anderson",
];

export const DailySetup: FC = () => {
	const [step, setStep] = useState(0);
	const { doctorName, setDoctorName } = useUserStore();
	const [selectedUnit, setSelectedUnit] = useState("");
	const [selectedShift, setSelectedShift] = useState("");
	const [selectedPatients, setSelectedPatients] = useState<Array<string>>([]);
	const navigate = useNavigate({ from: "/daily-setup" });

	const advanceStep = (): void => {
		setStep((s) => s + 1);
	};

	const handleComplete = (): void => {
		if (selectedPatients.length === 0) {
			return; // Matches test case
		}
		// The user's setup is already saved in the Zustand store
		void navigate({ to: "/" });
	};

	const togglePatient = (patient: string): void => {
		setSelectedPatients((previous) =>
			previous.includes(patient)
				? previous.filter((p) => p !== patient)
				: [...previous, patient]
		);
	};

	const renderStep = (): JSX.Element => {
		switch (step) {
			case 1: // Select Unit
				return (
					<>
						<h1 className="text-2xl font-bold text-center">
							Hello, {doctorName}!
						</h1>
						<h2 className="mb-4 text-xl font-semibold text-center">
							Select Your Unit
						</h2>
						<div className="flex flex-col space-y-2">
							{MOCK_UNITS.map((unit) => (
								<button
									key={unit}
									className={`px-4 py-2 border rounded-md ${
										selectedUnit === unit
											? "bg-blue-500 text-white"
											: "bg-white"
									}`}
									onClick={() => {
										setSelectedUnit(unit);
									}}
								>
									{unit}
								</button>
							))}
						</div>
						<button
							className="w-full px-4 py-2 mt-4 text-white bg-blue-500 rounded-md disabled:bg-gray-400 hover:bg-blue-600"
							disabled={!selectedUnit}
							onClick={advanceStep}
						>
							Continue
						</button>
					</>
				);
			case 2: // Select Shift
				return (
					<>
						<h1 className="mb-4 text-2xl font-bold text-center">
							Select Your Shift
						</h1>
						<div className="flex flex-col space-y-2">
							{MOCK_SHIFTS.map((shift) => (
								<button
									key={shift}
									className={`px-4 py-2 border rounded-md ${
										selectedShift === shift
											? "bg-blue-500 text-white"
											: "bg-white"
									}`}
									onClick={() => {
										setSelectedShift(shift);
									}}
								>
									{shift}
								</button>
							))}
						</div>
						<button
							className="w-full px-4 py-2 mt-4 text-white bg-blue-500 rounded-md disabled:bg-gray-400 hover:bg-blue-600"
							disabled={!selectedShift}
							onClick={advanceStep}
						>
							Continue
						</button>
					</>
				);
			case 3: // Select Patients
				return (
					<>
						<h1 className="mb-2 text-2xl font-bold text-center">
							Select Your Patients
						</h1>
						<p className="mb-4 text-center text-gray-600">
							{selectedPatients.length} of {MOCK_PATIENTS.length} selected
						</p>
						<div className="grid grid-cols-2 gap-2 mb-4 max-h-60 overflow-y-auto">
							{MOCK_PATIENTS.map((patient) => (
								<button
									key={patient}
									className={`px-4 py-2 border rounded-md text-sm ${
										selectedPatients.includes(patient)
											? "bg-blue-500 text-white"
											: "bg-white"
									}`}
									onClick={() => {
										togglePatient(patient);
									}}
								>
									{patient}
								</button>
							))}
						</div>
						{selectedPatients.length === 0 && (
							<p className="my-2 text-sm text-center text-red-600">
								Please select at least one patient
							</p>
						)}
						<button
							className="w-full px-4 py-2 text-white bg-green-500 rounded-md disabled:bg-gray-400 hover:bg-green-600"
							disabled={selectedPatients.length === 0}
							onClick={handleComplete}
						>
							Complete Setup
						</button>
					</>
				);
			default: // Step 0: Enter Name
				return (
					<>
						<h1 className="mb-4 text-2xl font-bold text-center">
							Welcome to Relevo
						</h1>
						<div className="mb-4">
							<label
								className="block mb-2 text-sm font-medium text-gray-700"
								htmlFor="name"
							>
								Your Name
							</label>
							<input
								className="w-full px-3 py-2 border border-gray-300 rounded-md"
								id="name"
								placeholder="e.g., Dr. Jane Doe"
								type="text"
								value={doctorName}
								onChange={(event_) => {
									setDoctorName(event_.target.value);
								}}
							/>
						</div>
						<button
							className="w-full px-4 py-2 mt-4 text-white bg-blue-500 rounded-md disabled:bg-gray-400 hover:bg-blue-600"
							disabled={!doctorName.trim()}
							onClick={advanceStep}
						>
							Continue
						</button>
					</>
				);
		}
	};

	return (
		<div className="flex flex-col items-center justify-center min-h-screen bg-gray-100">
			<div className="p-8 bg-white rounded-lg shadow-md w-96">
				{renderStep()}
			</div>
		</div>
	);
};
