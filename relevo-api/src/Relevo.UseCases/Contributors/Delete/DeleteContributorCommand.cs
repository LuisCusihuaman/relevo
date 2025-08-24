using Ardalis.Result;
using Ardalis.SharedKernel;

namespace Relevo.UseCases.Contributors.Delete;

public record DeleteContributorCommand(int ContributorId) : ICommand<Result>;
