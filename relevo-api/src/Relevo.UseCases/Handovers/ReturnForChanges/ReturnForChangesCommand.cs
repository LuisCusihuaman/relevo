using Ardalis.SharedKernel;

namespace Relevo.UseCases.Handovers.ReturnForChanges;

public record ReturnForChangesCommand(string HandoverId, string UserId) : ICommand<Ardalis.Result.Result>;

