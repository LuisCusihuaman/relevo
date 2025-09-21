using Ardalis.HttpClientTestExtensions;
using Relevo.Web.Setup;
using Relevo.Web.Models;
using Relevo.Web.Me;
using Relevo.Core.Interfaces;
using Xunit;
using System.Net;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class SetupEndpoints(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateClient();

  [Fact]
  public async Task GetUnits_ReturnsUnits()
  {
    var result = await _client.GetAndDeserializeAsync<UnitListResponse>("/setup/units");
    Assert.NotNull(result);
    Assert.NotEmpty(result.Units);
    Assert.Contains(result.Units, u => u.Id == "unit-1");
  }

  [Fact]
  public async Task GetShifts_ReturnsShifts()
  {
    var result = await _client.GetAndDeserializeAsync<ShiftListResponse>("/setup/shifts");
    Assert.NotNull(result);
    Assert.NotEmpty(result.Shifts);
    Assert.Contains(result.Shifts, s => s.Id == "shift-day");
  }

  [Fact]
  public async Task GetActiveHandover_ReturnsHandover_WhenUserHasActiveHandover()
  {
    var result = await _client.GetAndDeserializeAsync<GetActiveHandoverResponse>("/me/handovers/active");
    Assert.NotNull(result);
    // This test will pass once we have seed data for active handovers
    // For now, it should return 404 or empty result
  }

  [Fact]
  public async Task GetUserPreferences_ReturnsPreferences_WhenUserExists()
  {
    var result = await _client.GetAndDeserializeAsync<UserPreferencesRecord>("/me/preferences");
    Assert.NotNull(result);
    // Test will validate user preferences are returned correctly
  }

  [Fact]
  public async Task GetUserSessions_ReturnsSessions_WhenUserExists()
  {
    var result = await _client.GetAndDeserializeAsync<IReadOnlyList<UserSessionRecord>>("/me/sessions");
    Assert.NotNull(result);
    // Test will validate user sessions are returned correctly
  }

}


