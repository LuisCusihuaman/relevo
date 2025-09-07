using Relevo.Core.Interfaces;

namespace Relevo.UseCases.Setup;

public class GetMyHandoversUseCase
{
    private readonly ISetupRepository _repository;

    public GetMyHandoversUseCase(ISetupRepository repository)
    {
        _repository = repository;
    }

    public async Task<(IReadOnlyList<HandoverRecord> Handovers, int TotalCount)> ExecuteAsync(
        string userId,
        int page,
        int pageSize)
    {
        return await Task.FromResult(_repository.GetMyHandovers(userId, page, pageSize));
    }
}
