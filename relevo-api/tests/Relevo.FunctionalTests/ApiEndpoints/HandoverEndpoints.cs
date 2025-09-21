using Ardalis.HttpClientTestExtensions;
using Relevo.Web.Me;
using Xunit;
using System.Net;
using System.Collections.Generic;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class HandoverEndpoints(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateClient();

  [Fact]
  public async Task GetActiveHandover_ReturnsData_WhenActiveHandoverExists()
  {
    var response = await _client.GetAsync("/me/handovers/active");
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    // With seed data, we expect to get a handover
  }



}
