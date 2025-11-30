using Ardalis.HttpClientTestExtensions;
using Relevo.Web;
using Relevo.Web.ShiftCheckIn;
using Xunit;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class ShiftCheckInGetUnits(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateAuthenticatedClient();

  [Fact]
  public async Task ReturnsUnits()
  {
    var unitId = DapperTestSeeder.UnitId;
    
    var result = await _client.GetAndDeserializeAsync<UnitListResponse>("/shift-check-in/units");

    Assert.NotNull(result);
    Assert.NotEmpty(result.Units);
    Assert.Contains(result.Units, u => u.Id == unitId);
  }
}
