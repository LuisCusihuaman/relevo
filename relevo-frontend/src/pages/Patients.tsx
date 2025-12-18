import { useMemo, useState, useCallback, type ReactElement } from "react";
import { useNavigate } from "@tanstack/react-router";
import {
	EntityListMobile,
	EntityTable,
	FilterToolbar,
	ListHeader,
} from "@/components/home";
import type { HandoverUI as Handover } from "@/types/domain";
import { useAllPatients, usePatientsByUnitForList, mapApiPatientToUiHandover, useUnits } from "@/api";

export function Patients(): ReactElement {
	const navigate = useNavigate();
	const [selectedUnit, setSelectedUnit] = useState<string | null>(null);

	const handleUnitChange = useCallback((unitId: string | null): void => {
		setSelectedUnit(unitId);
	}, []);

	const { data: allPatientsData, isLoading: isLoadingAll, error: errorAll } = useAllPatients();
	const { data: unitPatientsData, isLoading: isLoadingUnit, error: errorUnit } = usePatientsByUnitForList(
		selectedUnit
	);
	const { data: units } = useUnits();

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
	const handovers: ReadonlyArray<Handover> = useMemo(() => {
		if (!patientsData?.items) return [];
		return patientsData.items.map(mapApiPatientToUiHandover);
	}, [patientsData]);

	const handleHandoverClick = (
		handoverId: string,
		_patientName: string,
	): void => {
		// Navigate to patient page - handoverId here is actually the patient ID
		// The patient page will resolve the active handover
		void navigate({ 
			to: "/patient/$patientId", 
			params: { patientId: handoverId } 
		});
	};

	// Show loading state
	if (isLoading) {
		return (
		<div className="mx-auto my-6 min-h-[calc(100vh-366px)] w-[var(--geist-page-width-with-margin)] max-w-full px-6 py-0 md:min-h-[calc(100vh-273px)]">
			<ListHeader />
			<FilterToolbar selectedUnit={selectedUnit} onUnitChange={handleUnitChange} />
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
			<FilterToolbar selectedUnit={selectedUnit} onUnitChange={handleUnitChange} />
			<EntityTable handleHandoverClick={handleHandoverClick} handovers={handovers} unitName={selectedUnitName} />
			<EntityListMobile handleHandoverClick={handleHandoverClick} handovers={handovers} unitName={selectedUnitName} />
		</div>
	);
}
