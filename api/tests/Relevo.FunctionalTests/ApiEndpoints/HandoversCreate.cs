using Ardalis.HttpClientTestExtensions;
using Relevo.Web;
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
    var patientId = DapperTestSeeder.PatientId1;
    var userId = DapperTestSeeder.UserId;
    var shiftDayId = DapperTestSeeder.ShiftDayId;
    var shiftNightId = DapperTestSeeder.ShiftNightId;
    
    var request = new CreateHandoverRequestDto
    {
        PatientId = patientId,
        FromDoctorId = userId,
        ToDoctorId = userId,
        FromShiftId = shiftDayId,
        ToShiftId = shiftNightId,
        InitiatedBy = userId,
        Notes = "Functional Test"
    };

    var response = await _client.PostAsJsonAsync("/handovers", request);
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    
    var result = await response.Content.ReadFromJsonAsync<CreateHandoverResponse>();

    Assert.NotNull(result);
    Assert.NotNull(result.Id);
    Assert.Equal(patientId, result.PatientId);
    Assert.Equal("Draft", result.Status);
  }
}
