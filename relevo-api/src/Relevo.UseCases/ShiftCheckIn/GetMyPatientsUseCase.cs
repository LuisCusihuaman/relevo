using System.Collections.Generic;
using Relevo.Core.Interfaces;

namespace Relevo.UseCases.ShiftCheckIn;

public class GetMyPatientsUseCase
{
    private readonly IAssignmentRepository _repository;

    public GetMyPatientsUseCase(IAssignmentRepository repository)
    {
        _repository = repository;
    }

    public (IReadOnlyList<PatientRecord> Patients, int TotalCount) Execute(string userId, int page, int pageSize)
    {
        return _repository.GetMyPatients(userId, page, pageSize);
    }
}
