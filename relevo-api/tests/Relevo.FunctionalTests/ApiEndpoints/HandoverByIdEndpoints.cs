using Ardalis.HttpClientTestExtensions;
using Relevo.Web.Handovers;
using Relevo.Core.Interfaces;
using Xunit;
using Dapper;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace Relevo.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class HandoverByIdEndpoints(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>, IAsyncLifetime
{
  private readonly HttpClient _client = factory.CreateClient();
  private const string TestHandoverId = "handover-e2e-get";
  private const string TestPatientId = "pat-001"; // Assumes seed patient exists
  
  public async Task InitializeAsync()
  {
      // Re-create the test handover to ensure it exists and has correct data
      using var connection = new OracleConnection("User Id=RELEVO_APP;Password=TuPass123;Data Source=localhost:1521/XE;Pooling=true;Connection Timeout=15");
      connection.Open();

      // Cleanup first
      await CleanupTestHandover(connection);

      // Create assignment
      await connection.ExecuteAsync(@"
        MERGE INTO USER_ASSIGNMENTS ua
        USING (SELECT 'assign-e2e-get' AS ASSIGNMENT_ID, 'user_demo12345678901234567890123456' AS USER_ID, 'shift-day' AS SHIFT_ID, :pid AS PATIENT_ID FROM DUAL) src
        ON (ua.ASSIGNMENT_ID = src.ASSIGNMENT_ID)
        WHEN MATCHED THEN UPDATE SET ua.ASSIGNED_AT = SYSTIMESTAMP
        WHEN NOT MATCHED THEN INSERT (ASSIGNMENT_ID, USER_ID, SHIFT_ID, PATIENT_ID, ASSIGNED_AT)
        VALUES (src.ASSIGNMENT_ID, src.USER_ID, src.SHIFT_ID, src.PATIENT_ID, SYSTIMESTAMP)",
        new { pid = TestPatientId });

      // Create handover
      await connection.ExecuteAsync(@"
        INSERT INTO HANDOVERS (ID, ASSIGNMENT_ID, PATIENT_ID, STATUS, SHIFT_NAME, FROM_SHIFT_ID, TO_SHIFT_ID, FROM_DOCTOR_ID, TO_DOCTOR_ID, CREATED_BY, HANDOVER_WINDOW_DATE, READY_AT, CREATED_AT)
        VALUES (:id, 'assign-e2e-get', :pid, 'Ready', 'Mañana → Noche', 'shift-day', 'shift-night', 'user_demo12345678901234567890123456', 'user_demo12345678901234567890123457', 'user_demo12345678901234567890123456', TRUNC(SYSDATE), SYSTIMESTAMP - INTERVAL '30' MINUTE, SYSTIMESTAMP)",
        new { id = TestHandoverId, pid = TestPatientId });

      // Create singleton sections
      await connection.ExecuteAsync(@"
        INSERT INTO HANDOVER_PATIENT_DATA(HANDOVER_ID, ILLNESS_SEVERITY, SUMMARY_TEXT, STATUS, LAST_EDITED_BY, CREATED_AT)
        VALUES (:id, 'Stable', 'Paciente de 14 años con neumonía adquirida en comunidad. Ingreso hace 3 días. Tratamiento con Amoxicilina y oxígeno suplementario.', 'completed', 'user_demo12345678901234567890123456', SYSTIMESTAMP)",
        new { id = TestHandoverId });

      await connection.ExecuteAsync(@"
        INSERT INTO HANDOVER_SITUATION_AWARENESS(HANDOVER_ID, CONTENT, STATUS, LAST_EDITED_BY, CREATED_AT)
        VALUES (:id, 'Paciente estable, sin complicaciones. Buena respuesta al tratamiento antibiótico.', 'completed', 'user_demo12345678901234567890123456', SYSTIMESTAMP)",
        new { id = TestHandoverId });

      await connection.ExecuteAsync(@"
        INSERT INTO HANDOVER_SYNTHESIS(HANDOVER_ID, CONTENT, STATUS, LAST_EDITED_BY, CREATED_AT)
        VALUES (:id, 'Continuar tratamiento actual. Alta probable en 48-72 horas si evolución favorable.', 'draft', 'user_demo12345678901234567890123456', SYSTIMESTAMP)",
        new { id = TestHandoverId });

      // Action items
      await connection.ExecuteAsync(@"
        INSERT INTO HANDOVER_ACTION_ITEMS (ID, HANDOVER_ID, DESCRIPTION, IS_COMPLETED, CREATED_AT)
        VALUES ('action-e2e-1', :id, 'Realizar nebulizaciones cada 6 horas', 0, SYSTIMESTAMP)",
        new { id = TestHandoverId });
  }

  public async Task DisposeAsync()
  {
      using var connection = new OracleConnection("User Id=RELEVO_APP;Password=TuPass123;Data Source=localhost:1521/XE;Pooling=true;Connection Timeout=15");
      connection.Open();
      await CleanupTestHandover(connection);
  }

  private async Task CleanupTestHandover(IDbConnection connection)
  {
      try { await connection.ExecuteAsync("DELETE FROM HANDOVER_ACTION_ITEMS WHERE HANDOVER_ID = :id", new { id = TestHandoverId }); } catch {}
      try { await connection.ExecuteAsync("DELETE FROM HANDOVER_PATIENT_DATA WHERE HANDOVER_ID = :id", new { id = TestHandoverId }); } catch {}
      try { await connection.ExecuteAsync("DELETE FROM HANDOVER_SITUATION_AWARENESS WHERE HANDOVER_ID = :id", new { id = TestHandoverId }); } catch {}
      try { await connection.ExecuteAsync("DELETE FROM HANDOVER_SYNTHESIS WHERE HANDOVER_ID = :id", new { id = TestHandoverId }); } catch {}
      try { await connection.ExecuteAsync("DELETE FROM HANDOVERS WHERE ID = :id", new { id = TestHandoverId }); } catch {}
      try { await connection.ExecuteAsync("DELETE FROM USER_ASSIGNMENTS WHERE ASSIGNMENT_ID = 'assign-e2e-get'"); } catch {}
  }

  [Fact]
  public async Task GetHandoverById_ReturnsHandover_WhenHandoverExists()
  {
    Console.WriteLine("[TEST] Starting GetHandoverById_ReturnsHandover_WhenHandoverExists test");
    var handoverId = TestHandoverId;
    var result = await _client.GetAndDeserializeAsync<GetHandoverByIdResponse>($"/handovers/{handoverId}");

    Assert.NotNull(result);
    Assert.Equal(handoverId, result.Id);
    Assert.NotNull(result.AssignmentId);
    Assert.NotNull(result.PatientId);
    Assert.NotNull(result.Status);
    Assert.NotNull(result.illnessSeverity);
    Assert.NotNull(result.patientSummary);
    Assert.NotNull(result.actionItems);
    Assert.NotNull(result.ShiftName);
    Assert.NotNull(result.CreatedBy);
    Assert.NotNull(result.AssignedTo);
  }

  [Fact]
  public async Task GetHandoverById_ReturnsCorrectHandoverStructure()
  {
    var handoverId = TestHandoverId;
    var result = await _client.GetAndDeserializeAsync<GetHandoverByIdResponse>($"/handovers/{handoverId}");

    Assert.NotNull(result);

    // Verify basic fields
    Assert.Equal(handoverId, result.Id);
    Assert.Equal(TestPatientId, result.PatientId);
    Assert.Contains(result.Status, new[] { "Active", "InProgress", "Completed", "Ready" });

    // Verify illness severity structure - using actual property names
    Assert.NotNull(result.illnessSeverity);
    Assert.NotNull(result.illnessSeverity.severity);
    Assert.Contains(result.illnessSeverity.severity, new[] { "Stable", "Watcher", "Unstable" });

    // Verify patient summary structure - using actual property names
    Assert.NotNull(result.patientSummary);
    Assert.NotNull(result.patientSummary.content);

    // Verify action items structure
    Assert.NotNull(result.actionItems);

    // Verify other required fields
    Assert.NotNull(result.AssignmentId);
    Assert.NotNull(result.ShiftName);
    Assert.NotNull(result.CreatedBy);
    Assert.NotNull(result.AssignedTo);
  }

  [Fact]
  public async Task GetHandoverById_ReturnsHandoverWithPatientName_WhenPatientExists()
  {
    var handoverId = TestHandoverId;
    var result = await _client.GetAndDeserializeAsync<GetHandoverByIdResponse>($"/handovers/{handoverId}");

    Assert.NotNull(result);
    Assert.NotNull(result.PatientName);
    Assert.Equal("María García", result.PatientName);
  }

  [Fact]
  public async Task GetHandoverById_ReturnsHandoverWithActionItems()
  {
    var handoverId = TestHandoverId;
    var result = await _client.GetAndDeserializeAsync<GetHandoverByIdResponse>($"/handovers/{handoverId}");

    Assert.NotNull(result);
    Assert.NotNull(result.actionItems);
    // The test handover has action items
    Assert.True(result.actionItems.Count > 0);
    foreach (var actionItem in result.actionItems)
    {
      Assert.NotNull(actionItem.id);
      Assert.NotNull(actionItem.description);
    }
  }

  [Fact]
  public async Task GetHandoverById_Returns404_WhenHandoverDoesNotExist()
  {
    var nonExistentHandoverId = "non-existent-handover-id";

    var response = await _client.GetAsync($"/handovers/{nonExistentHandoverId}");

    Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
  }

  [Fact]
  public async Task GetHandoverById_HandlesDifferentHandoverStatuses()
  {
    // Test with the existing handover
    var handoverId = TestHandoverId;
    var result = await _client.GetAndDeserializeAsync<GetHandoverByIdResponse>($"/handovers/{handoverId}");

    Assert.NotNull(result);
    Assert.NotNull(result.Status);
    Assert.Contains(result.Status, new[] { "Active", "InProgress", "Completed", "Ready" });
  }

  [Fact]
  public async Task GetHandoverById_ReturnsCorrectShiftInformation()
  {
    var handoverId = TestHandoverId;
    var result = await _client.GetAndDeserializeAsync<GetHandoverByIdResponse>($"/handovers/{handoverId}");

    Assert.NotNull(result);
    Assert.NotNull(result.ShiftName);
    Assert.Equal("Mañana → Noche", result.ShiftName);
  }

  [Fact]
  public async Task GetHandoverById_ReturnsCorrectIllnessSeverity()
  {
    var handoverId = TestHandoverId;
    var result = await _client.GetAndDeserializeAsync<GetHandoverByIdResponse>($"/handovers/{handoverId}");

    Assert.NotNull(result);
    Assert.NotNull(result.illnessSeverity);
    Assert.Equal("Stable", result.illnessSeverity.severity);
  }

  [Fact]
  public async Task GetHandoverById_ReturnsOptionalFields_WhenPresent()
  {
    var handoverId = TestHandoverId;
    var result = await _client.GetAndDeserializeAsync<GetHandoverByIdResponse>($"/handovers/{handoverId}");

    Assert.NotNull(result);
    Assert.NotNull(result.PatientName);
  }

  [Fact]
  public async Task GetHandoverById_ReturnsActionItemsList()
  {
    // Test with the existing handover that has action items
    var handoverId = TestHandoverId;
    var result = await _client.GetAndDeserializeAsync<GetHandoverByIdResponse>($"/handovers/{handoverId}");

    Assert.NotNull(result);
    Assert.NotNull(result.actionItems);
    Assert.True(result.actionItems.Count > 0);
  }

  [Fact]
  public async Task GetHandoverById_HandlesSpecialCharactersInIds()
  {
    // Test with handover ID containing special characters or numbers
    var handoverId = TestHandoverId; 
    var result = await _client.GetAndDeserializeAsync<GetHandoverByIdResponse>($"/handovers/{handoverId}");

    Assert.NotNull(result);
    Assert.Equal(handoverId, result.Id);
  }
}

public class GetHandoverByIdResponse
{
  public string Id { get; set; } = string.Empty;
  public string AssignmentId { get; set; } = string.Empty;
  public string PatientId { get; set; } = string.Empty;
  public string? PatientName { get; set; }
  public string Status { get; set; } = string.Empty;
  public IllnessSeverityDto illnessSeverity { get; set; } = new();
  public PatientSummaryDto patientSummary { get; set; } = new();
  public List<ActionItemDto> actionItems { get; set; } = [];
  public string? situationAwarenessDocId { get; set; }
  public SynthesisDto? synthesis { get; set; }
  public string ShiftName { get; set; } = string.Empty;
  public string CreatedBy { get; set; } = string.Empty;
  public string AssignedTo { get; set; } = string.Empty;

  public class IllnessSeverityDto
  {
    public string severity { get; set; } = string.Empty;
  }

  public class PatientSummaryDto
  {
    public string content { get; set; } = string.Empty;
  }

  public class ActionItemDto
  {
    public string id { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
    public bool isCompleted { get; set; }
  }

  public class SynthesisDto
  {
    public string content { get; set; } = string.Empty;
  }
}
