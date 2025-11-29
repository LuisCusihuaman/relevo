using Ardalis.Result;
using MediatR;
using Relevo.Core.Models;

namespace Relevo.UseCases.Me.Handovers;

public record GetMyHandoversQuery(
    string UserId,
    int Page,
    int PageSize
) : IRequest<Result<(IReadOnlyList<HandoverRecord> Handovers, int TotalCount)>>;

