using Ardalis.HttpClientTestExtensions;
using Relevo.Web.Handovers;
using Xunit;
using System.Net;
using System.Net.Http.Json;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class HandoversCreate(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateClient();

  [Fact]
  public async Task CreatesHandover()
  {
    var request = new CreateHandoverRequestDto
    {
        PatientId = "pat-001",
        FromDoctorId = "dr-1",
        ToDoctorId = "dr-1",
        FromShiftId = "shift-day",
        ToShiftId = "shift-night",
        InitiatedBy = "dr-1",
        Notes = "Functional Test"
    };

    var response = await _client.PostAsJsonAsync("/handovers", request);
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    
    var result = await response.Content.ReadFromJsonAsync<CreateHandoverResponse>();

    Assert.NotNull(result);
    Assert.NotNull(result.Id);
    Assert.Equal("pat-001", result.PatientId);
    Assert.Equal("Draft", result.Status);
  }
}

