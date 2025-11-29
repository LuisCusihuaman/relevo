using System.Collections.Generic;
using Relevo.Core.Interfaces;

namespace Relevo.UseCases.ShiftCheckIn;

public class GetPatientsByUnitUseCase
{
    private readonly IPatientRepository _repository;

    public GetPatientsByUnitUseCase(IPatientRepository repository)
    {
        _repository = repository;
    }

    public (IReadOnlyList<PatientRecord> Patients, int TotalCount) Execute(string unitId, int page, int pageSize)
    {
        return _repository.GetPatientsByUnit(unitId, page, pageSize);
    }
}
