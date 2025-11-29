using System.Threading.Tasks;
using Relevo.Core.Interfaces;

namespace Relevo.UseCases.ShiftCheckIn;

public class GetHandoverByIdUseCase
{
    private readonly IHandoverRepository _repository;

    public GetHandoverByIdUseCase(IHandoverRepository repository)
    {
        _repository = repository;
    }

    public HandoverRecord? Execute(string handoverId)
    {
        return _repository.GetHandoverById(handoverId);
    }
}

