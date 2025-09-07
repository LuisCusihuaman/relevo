using Ardalis.Result;
using Relevo.Core.ContributorAggregate;
using Relevo.Core.ContributorAggregate.Events;
using Relevo.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Relevo.Core.Services;

/// <summary>
/// This is here mainly so there's an example of a domain service
/// and also to demonstrate how to fire domain events from a service.
/// Uses ContributorService with Dapper for data access.
/// </summary>
public class DeleteContributorService(IContributorService _service,
  IMediator _mediator,
  ILogger<DeleteContributorService> _logger) : IDeleteContributorService
{
  public async Task<Result> DeleteContributor(int contributorId)
  {
    _logger.LogInformation("Deleting Contributor {contributorId}", contributorId);
    Contributor? aggregateToDelete = await _service.GetByIdAsync(contributorId);
    if (aggregateToDelete == null) return Result.NotFound();

    await _service.DeleteAsync(contributorId);
    var domainEvent = new ContributorDeletedEvent(contributorId);
    await _mediator.Publish(domainEvent);

    return Result.Success();
  }
}
