using Ardalis.Result;
using MediatR;
using Relevo.Core.Models;

namespace Relevo.UseCases.Me.Activity;

public record GetHandoverActivityQuery(string HandoverId) : IRequest<Result<IReadOnlyList<HandoverActivityRecord>>>;

