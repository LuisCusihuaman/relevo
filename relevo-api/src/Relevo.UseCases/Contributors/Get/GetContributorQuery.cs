using Ardalis.Result;
using Ardalis.SharedKernel;

namespace Relevo.UseCases.Contributors.Get;

public record GetContributorQuery(int ContributorId) : IQuery<Result<ContributorDTO>>;
