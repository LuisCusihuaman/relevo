using Relevo.Core.ContributorAggregate;
using Relevo.Core.Interfaces;
using Relevo.Core.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Relevo.UnitTests.Core.Services;

public class DeleteContributorService_DeleteContributor
{
  private readonly IContributorService _contributorService = Substitute.For<IContributorService>();
  private readonly IMediator _mediator = Substitute.For<IMediator>();
  private readonly ILogger<DeleteContributorService> _logger = Substitute.For<ILogger<DeleteContributorService>>();

  private readonly DeleteContributorService _service;

  public DeleteContributorService_DeleteContributor()
  {
    _service = new DeleteContributorService(_contributorService, _mediator, _logger);
  }

  [Fact]
  public async Task ReturnsNotFoundGivenCantFindContributor()
  {
    _contributorService.GetByIdAsync(0)
      .Returns(Task.FromResult<Contributor?>(null));

    var result = await _service.DeleteContributor(0);

    Assert.Equal(Ardalis.Result.ResultStatus.NotFound, result.Status);
  }
}
