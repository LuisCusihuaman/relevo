using System.Collections.Generic;
using Relevo.Core.Interfaces;

namespace Relevo.UseCases.ShiftCheckIn;

public class GetMyHandoversUseCase
{
    private readonly IHandoverRepository _repository;

    public GetMyHandoversUseCase(IHandoverRepository repository)
    {
        _repository = repository;
    }

    public (IReadOnlyList<HandoverRecord> Handovers, int TotalCount) Execute(string userId, int page, int pageSize)
    {
        return _repository.GetMyHandovers(userId, page, pageSize);
    }
}
