using Ardalis.Result;
using MediatR;
using Relevo.Core.Models;

namespace Relevo.UseCases.Me.Checklists;

public record GetHandoverChecklistsQuery(string HandoverId) : IRequest<Result<IReadOnlyList<HandoverChecklistRecord>>>;

