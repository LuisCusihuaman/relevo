using System.Collections.Generic;
using Relevo.Core.Interfaces;

namespace Relevo.UseCases.ShiftCheckIn;

public class GetPatientHandoversUseCase
{
    private readonly IHandoverRepository _repository;

    public GetPatientHandoversUseCase(IHandoverRepository repository)
    {
        _repository = repository;
    }

    public (IReadOnlyList<HandoverRecord> Handovers, int TotalCount) Execute(string patientId, int page, int pageSize)
    {
        return _repository.GetPatientHandovers(patientId, page, pageSize);
    }
}
