import { useMemo, useState, useCallback, type ReactElement } from "react";
import { useNavigate } from "@tanstack/react-router";
import {
	EntityListMobile,
	EntityTable,
	FilterToolbar,
	ListHeader,
} from "@/components/home";
import type { HandoverUI as Handover } from "@/types/domain";
import { useAllPatients, usePatientsByUnitForList, mapApiPatientToUiHandover, useUnits, useAllUsers } from "@/api";

export function Patients(): ReactElement {
	const navigate = useNavigate();
	const [selectedUnit, setSelectedUnit] = useState<string | null>(null);
	const [selectedUser, setSelectedUser] = useState<string | null>(null);

	const handleUnitChange = useCallback((unitId: string | null): void => {
		setSelectedUnit(unitId);
	}, []);

	const handleUserChange = useCallback((userId: string | null): void => {
		setSelectedUser(userId);
	}, []);

	const { data: allPatientsData, isLoading: isLoadingAll, error: errorAll } = useAllPatients();
	const { data: unitPatientsData, isLoading: isLoadingUnit, error: errorUnit } = usePatientsByUnitForList(
		selectedUnit
	);
	const { data: units } = useUnits();
	const { data: users } = useAllUsers();

	// When a unit is selected, only use unit data (never fall back to allPatientsData)
	// When no unit is selected, use allPatientsData
	// Important: If unitPatientsData is undefined and we're not loading, show empty array
	// This prevents showing allPatientsData when a unit is selected
	const patientsData = selectedUnit 
		? (unitPatientsData || { items: [], pagination: { totalItems: 0, page: 1, pageSize: 25, totalPages: 0 } })
		: allPatientsData;
	const isLoading = selectedUnit ? isLoadingUnit : isLoadingAll;
	const error = selectedUnit ? errorUnit : errorAll;

	// Get unit name from selected unit ID
	const selectedUnitName = useMemo(() => {
		if (!selectedUnit || !units) return undefined;
		return units.find(u => u.id === selectedUnit)?.name;
	}, [selectedUnit, units]);

	// Memoize the mapped patients to handovers to avoid unnecessary re-computations
	// Filter by selected user if a user is selected
	const handovers: ReadonlyArray<Handover> = useMemo(() => {
		if (!patientsData?.items) return [];
		const mapped = patientsData.items.map(mapApiPatientToUiHandover);
		
		if (selectedUser === "unassigned") {
			// Filter for unassigned patients
			return mapped.filter((handover) => {
				const handoverAuthor = handover.author?.toLowerCase().trim();
				return !handoverAuthor || handoverAuthor === "system" || handoverAuthor.includes("[bot]") || handoverAuthor.includes("dependabot");
			});
		}
		
		if (selectedUser) {
			return mapped.filter((handover) => {
				// Compare by full name (case-insensitive)
				const handoverAuthor = handover.author?.toLowerCase().trim();
				const selectedUserData = users?.find((u) => u.id === selectedUser);
				const selectedUserName = selectedUserData?.fullName.toLowerCase().trim();
				return handoverAuthor === selectedUserName;
			});
		}
		
		return mapped;
	}, [patientsData, selectedUser, users]);

	const handleHandoverClick = (
		handoverId: string,
		_patientName: string,
		patientId?: string,
	): void => {
		// Use the actual patientId if provided, otherwise fallback to handoverId
		// Navigate to /patient/pat-010 format
		const idToUse = patientId || handoverId;
		void navigate({ 
			to: "/patient/$patientId", 
			params: { patientId: idToUse } 
		});
	};

	// Show loading state
	if (isLoading) {
		return (
		<div className="mx-auto my-6 min-h-[calc(100vh-366px)] w-[var(--geist-page-width-with-margin)] max-w-full px-6 py-0 md:min-h-[calc(100vh-273px)]">
			<ListHeader />
			<FilterToolbar 
				selectedUnit={selectedUnit} 
				onUnitChange={handleUnitChange}
				selectedUser={selectedUser}
				onUserChange={handleUserChange}
				users={users}
			/>
			<EntityTable loading handleHandoverClick={() => {}} handovers={[]} unitName={selectedUnitName} />
			<div className="mt-6">
				<EntityListMobile loading handleHandoverClick={() => {}} handovers={[]} unitName={selectedUnitName} />
			</div>
		</div>
		);
	}

	// Show error state
	if (error) {
		return (
			<div className="mx-auto my-6 min-h-[calc(100vh-366px)] w-[var(--geist-page-width-with-margin)] max-w-full px-6 py-0 md:min-h-[calc(100vh-273px)]">
				<div className="flex items-center justify-center h-64">
					<div className="text-center">
						<div className="text-red-600 mb-2">Error al cargar traspasos</div>
						<p className="text-gray-600">Intente recargar la página</p>
					</div>
				</div>
			</div>
		);
	}

	return (
		<div className="mx-auto my-6 min-h-[calc(100vh-366px)] w-[var(--geist-page-width-with-margin)] max-w-full px-6 py-0 md:min-h-[calc(100vh-273px)]">
			<ListHeader />
			<FilterToolbar 
				selectedUnit={selectedUnit} 
				onUnitChange={handleUnitChange}
				selectedUser={selectedUser}
				onUserChange={handleUserChange}
				users={users}
			/>
			<EntityTable handleHandoverClick={handleHandoverClick} handovers={handovers} unitName={selectedUnitName} />
			<EntityListMobile handleHandoverClick={handleHandoverClick} handovers={handovers} unitName={selectedUnitName} />
		</div>
	);
}
