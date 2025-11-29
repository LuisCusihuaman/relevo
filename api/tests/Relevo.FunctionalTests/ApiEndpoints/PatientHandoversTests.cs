using System.Net.Http.Json;
using Relevo.Web.Patients;
using Relevo.Core.Interfaces;
using Xunit;
using Ardalis.HttpClientTestExtensions;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("TestCollection")]
public class PatientHandoversTests(CustomWebApplicationFactory<Program> factory) 
    : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetPatientHandovers_ReturnsEmptyList_WhenPatientHasNoHandovers()
    {
        var patientId = "pat-999-non-existent"; 
        var result = await _client.GetAndDeserializeAsync<GetPatientHandoversResponse>($"/patients/{patientId}/handovers");

        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        Assert.Empty(result.Items);
        Assert.Equal(0, result.Pagination.TotalItems);
    }
}

// Define response DTO if not available in Relevo.Web directly (it's likely in Relevo.Web.Patients)
// Checking code... Relevo.Web.Patients.GetPatientHandoversResponse exists.

