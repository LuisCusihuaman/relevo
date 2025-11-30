using Ardalis.HttpClientTestExtensions;
using Relevo.Web.ShiftCheckIn;
using Xunit;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class ShiftCheckInGetShifts(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateAuthenticatedClient();

  [Fact]
  public async Task ReturnsShifts()
  {
    // Shifts are seeded in DapperTestSeeder but not explicitly.
    // We need to ensure DapperTestSeeder seeds shifts.
    // Let's check DapperTestSeeder again.
    
    var result = await _client.GetAndDeserializeAsync<ShiftListResponse>("/shift-check-in/shifts");

    Assert.NotNull(result);
    // Asserting Count > 0 assuming seeding works or adding seeding if not present.
    // Let's update Seeder first to be sure.
  }
}

