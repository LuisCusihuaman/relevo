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

  // Active handover endpoint has been removed from the system
  // The concept of "active handover" is no longer part of the state machine

}
