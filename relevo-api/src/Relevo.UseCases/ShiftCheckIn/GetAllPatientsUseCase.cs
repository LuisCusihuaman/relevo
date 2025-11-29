using System.Collections.Generic;
using Relevo.Core.Interfaces;

namespace Relevo.UseCases.ShiftCheckIn;

public class GetAllPatientsUseCase
{
    private readonly IPatientRepository _repository;

    public GetAllPatientsUseCase(IPatientRepository repository)
    {
        _repository = repository;
    }

    public (IReadOnlyList<PatientRecord> Patients, int TotalCount) Execute(int page, int pageSize)
    {
        return _repository.GetAllPatients(page, pageSize);
    }
}
