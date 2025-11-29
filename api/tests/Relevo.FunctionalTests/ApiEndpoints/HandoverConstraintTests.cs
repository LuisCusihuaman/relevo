using System.Net;
using System.Net.Http.Json;
using Relevo.Web;
using Relevo.Web.Handovers;
using Xunit;
using Relevo.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Dapper;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("TestCollection")]
public class HandoverConstraintTests(CustomWebApplicationFactory<Program> factory) 
    : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();
    private static readonly string TestRunId = Guid.NewGuid().ToString()[..8];

    [Fact]
    public async Task Parallel_Creation_Same_Patient_Fails_With_Conflict()
    {
        var patientId = $"pat-const-{TestRunId}";
        
        // Insert test patient via Dapper
        using (var scope = factory.Services.CreateScope())
        {
            var dbFactory = scope.ServiceProvider.GetRequiredService<DapperConnectionFactory>();
            using var conn = dbFactory.CreateConnection();
            await conn.ExecuteAsync(@"
                INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, CREATED_AT, UPDATED_AT)
                VALUES (:Id, :Name, :UnitId, :DateOfBirth, :Gender, :AdmissionDate, :RoomNumber, :Diagnosis, SYSTIMESTAMP, SYSTIMESTAMP)",
                new { 
                    Id = patientId, 
                    Name = $"Constraint Patient {TestRunId}", 
                    UnitId = DapperTestSeeder.UnitId, 
                    DateOfBirth = new DateTime(2010, 1, 1), 
                    Gender = "Male", 
                    AdmissionDate = DateTime.Now.AddDays(-1), 
                    RoomNumber = "101", 
                    Diagnosis = "Test" 
                });
        }

        var createRequest = new CreateHandoverRequestDto
        {
            PatientId = patientId,
            FromDoctorId = DapperTestSeeder.UserId,
            ToDoctorId = DapperTestSeeder.UserId,
            FromShiftId = DapperTestSeeder.ShiftDayId,
            ToShiftId = DapperTestSeeder.ShiftNightId,
            InitiatedBy = DapperTestSeeder.UserId,
            Notes = "Constraint Test"
        };

        // 1. Create First Handover
        var response1 = await _client.PostAsJsonAsync("/handovers", createRequest);
        response1.EnsureSuccessStatusCode();

        // 2. Create Duplicate Handover (Same Patient, Same Shift Window)
        var response2 = await _client.PostAsJsonAsync("/handovers", createRequest);

        // 3. Assert Failure
        // Expecting 500 (ORA-00001) or 409 if handled. 
        // The legacy test accepted InternalServerError.
        Assert.False(response2.IsSuccessStatusCode);
        Assert.True(
            response2.StatusCode == HttpStatusCode.InternalServerError || 
            response2.StatusCode == HttpStatusCode.Conflict,
            $"Expected Conflict or InternalServerError, got {response2.StatusCode}");
    }
}

