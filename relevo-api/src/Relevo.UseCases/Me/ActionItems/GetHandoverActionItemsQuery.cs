using Ardalis.Result;
using MediatR;
using Relevo.Core.Models;

namespace Relevo.UseCases.Me.ActionItems;

public record GetHandoverActionItemsQuery(string HandoverId) : IRequest<Result<IReadOnlyList<HandoverActionItemFullRecord>>>;

