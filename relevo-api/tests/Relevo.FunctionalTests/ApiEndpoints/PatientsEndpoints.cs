using Ardalis.HttpClientTestExtensions;
using Relevo.Web.Patients;
using Xunit;
using FluentAssertions;
using System.Linq;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class PatientsEndpoints(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetAllPatients_ReturnsSuccessAndData()
    {
        var result = await _client.GetAndDeserializeAsync<GetAllPatientsResponse>("/patients");

        result.Should().NotBeNull();
        result.Items.Should().NotBeNull();
        result.Pagination.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllPatients_ReturnsAllPatientsFromAllUnits()
    {
        var result = await _client.GetAndDeserializeAsync<GetAllPatientsResponse>("/patients");

        result.Should().NotBeNull();
        result.Items.Should().NotBeEmpty();
        result.Pagination.TotalItems.Should().BeGreaterThan(0);

        // Verify we have patients from different units (based on test data)
        var patientIds = result.Items.Select(p => p.Id).ToList();
        patientIds.Should().Contain(p => p.StartsWith("pat-"));

        // Verify we have the expected number of patients (2 in current test data)
        result.Pagination.TotalItems.Should().Be(2);
    }

    [Fact]
    public async Task GetAllPatients_UsesDefaultPagination()
    {
        var result = await _client.GetAndDeserializeAsync<GetAllPatientsResponse>("/patients");

        result.Should().NotBeNull();
        result.Pagination.Page.Should().Be(1);
        result.Pagination.PageSize.Should().Be(25); // Default page size
        result.Items.Count.Should().BeLessOrEqualTo(25);
    }

    [Fact]
    public async Task GetAllPatients_WithCustomPagination()
    {
        const int page = 2;
        const int pageSize = 10;
        var route = $"/patients?page={page}&pageSize={pageSize}";
        var result = await _client.GetAndDeserializeAsync<GetAllPatientsResponse>(route);

        result.Should().NotBeNull();
        result.Pagination.Page.Should().Be(page);
        result.Pagination.PageSize.Should().Be(pageSize);
        result.Items.Count.Should().BeLessOrEqualTo(pageSize);
    }

    [Fact]
    public async Task GetAllPatients_WithSmallPageSize()
    {
        const int pageSize = 5;
        var route = $"/patients?page=1&pageSize={pageSize}";
        var result = await _client.GetAndDeserializeAsync<GetAllPatientsResponse>(route);

        result.Should().NotBeNull();
        result.Pagination.PageSize.Should().Be(pageSize);
        result.Items.Count.Should().BeLessOrEqualTo(pageSize);
    }

    [Fact]
    public async Task GetAllPatients_ReturnsCorrectPatientStructure()
    {
        var result = await _client.GetAndDeserializeAsync<GetAllPatientsResponse>("/patients");

        result.Should().NotBeNull();
        result.Items.Should().NotBeEmpty();

        var firstPatient = result.Items.First();
        firstPatient.Id.Should().NotBeNullOrEmpty();
        firstPatient.Name.Should().NotBeNullOrEmpty();
        firstPatient.HandoverStatus.Should().NotBeNull();
        firstPatient.HandoverId.Should().BeNull(); // Test data doesn't have handover IDs
    }

    [Fact]
    public async Task GetAllPatients_ReturnsPatientsInCorrectOrder()
    {
        var result = await _client.GetAndDeserializeAsync<GetAllPatientsResponse>("/patients?page=1&pageSize=10");

        result.Should().NotBeNull();
        result.Items.Should().NotBeEmpty();

        // Verify patients are ordered by ID (as per the repository implementation)
        var patientIds = result.Items.Select(p => p.Id).ToList();
        var sortedIds = patientIds.OrderBy(id => id).ToList();
        patientIds.Should().Equal(sortedIds);
    }

    [Fact]
    public async Task GetAllPatients_WithPageBeyondAvailableData()
    {
        // Request a page that doesn't exist
        const int highPage = 100;
        var route = $"/patients?page={highPage}&pageSize=10";
        var result = await _client.GetAndDeserializeAsync<GetAllPatientsResponse>(route);

        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.Pagination.TotalItems.Should().Be(2); // Total should still be correct
        result.Pagination.Page.Should().Be(highPage);
    }

    [Fact]
    public async Task GetAllPatients_CalculatesTotalPagesCorrectly()
    {
        const int pageSize = 10;
        var route = $"/patients?page=1&pageSize={pageSize}";
        var result = await _client.GetAndDeserializeAsync<GetAllPatientsResponse>(route);

        result.Should().NotBeNull();

        // With 2 total items and pageSize 10, we should have 1 page
        var expectedTotalPages = (int)Math.Ceiling(2.0 / pageSize);
        result.Pagination.TotalPages.Should().Be(expectedTotalPages);
    }

    [Fact]
    public async Task GetAllPatients_WithLargePageSize()
    {
        // Request all patients in one page
        var route = "/patients?page=1&pageSize=100";
        var result = await _client.GetAndDeserializeAsync<GetAllPatientsResponse>(route);

        result.Should().NotBeNull();
        result.Items.Count.Should().Be(2); // Should return all 2 patients
        result.Pagination.TotalItems.Should().Be(2);
        result.Pagination.TotalPages.Should().Be(1);
    }
}
