using Ardalis.Result;
using Ardalis.SharedKernel;
using Relevo.Core.Models;

namespace Relevo.UseCases.Handovers.GetPending;

public record GetPendingHandoversQuery(string UserId) : IQuery<Result<IReadOnlyList<HandoverRecord>>>;

