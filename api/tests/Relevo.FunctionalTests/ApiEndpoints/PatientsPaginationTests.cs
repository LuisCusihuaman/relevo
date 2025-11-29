using System.Net.Http.Json;
using Relevo.Web.Patients;
using Xunit;
using Ardalis.HttpClientTestExtensions;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("TestCollection")]
public class PatientsPaginationTests(CustomWebApplicationFactory<Program> factory) 
    : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetAllPatients_WithPageBeyondAvailableData_ReturnsEmptyList()
    {
        // Request a page that definitely doesn't exist
        const int highPage = 1000;
        var route = $"/patients?page={highPage}&pageSize=10";
        var result = await _client.GetAndDeserializeAsync<GetAllPatientsResponse>(route);

        Assert.NotNull(result);
        Assert.Empty(result.Items);
        Assert.NotNull(result.Pagination);
        // Total items might be > 0 if seed data exists
        Assert.Equal(highPage, result.Pagination.Page);
    }

    [Fact]
    public async Task GetAllPatients_WithCustomPagination_ReturnsCorrectPageSize()
    {
        const int pageSize = 5;
        var route = $"/patients?page=1&pageSize={pageSize}";
        var result = await _client.GetAndDeserializeAsync<GetAllPatientsResponse>(route);

        Assert.NotNull(result);
        Assert.NotNull(result.Pagination);
        Assert.Equal(pageSize, result.Pagination.PageSize);
        Assert.True(result.Items.Count <= pageSize);
    }
}

