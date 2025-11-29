using Ardalis.HttpClientTestExtensions;
using Relevo.Web.ShiftCheckIn;
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



}


