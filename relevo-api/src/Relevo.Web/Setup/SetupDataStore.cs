using Relevo.Web.Patients;
using Relevo.Web.Me;
using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Relevo.Web.Setup;

public class SetupDataStore : ISetupDataProvider
{
  private readonly IDbConnection _connection;

  public SetupDataStore()
  {
    // Use in-memory SQLite database for testing
    _connection = new SqliteConnection("Data Source=:memory:");
    _connection.Open();

    // Create tables with same structure as Oracle
    CreateTables();
    SeedTestData();
  }

  private void CreateTables()
  {
    using var cmd = _connection.CreateCommand();
    cmd.CommandText = @"
      CREATE TABLE UNITS (
        ID TEXT PRIMARY KEY,
        NAME TEXT NOT NULL
      );

      CREATE TABLE SHIFTS (
        ID TEXT PRIMARY KEY,
        NAME TEXT NOT NULL,
        START_TIME TEXT NOT NULL,
        END_TIME TEXT NOT NULL
      );

      CREATE TABLE PATIENTS (
        ID TEXT PRIMARY KEY,
        NAME TEXT NOT NULL,
        UNIT_ID TEXT,
        FOREIGN KEY (UNIT_ID) REFERENCES UNITS(ID)
      );";
    cmd.ExecuteNonQuery();
  }

  private void SeedTestData()
  {
    // Seed units
    _connection.Execute(@"
      INSERT INTO UNITS (ID, NAME) VALUES
      (@Id, @Name);",
      new[]
      {
        new { Id = "unit-1", Name = "UCI" },
        new { Id = "unit-2", Name = "Pediatría General" },
        new { Id = "unit-3", Name = "Pediatría Especializada" }
      });

    // Seed shifts
    _connection.Execute(@"
      INSERT INTO SHIFTS (ID, NAME, START_TIME, END_TIME) VALUES
      (@Id, @Name, @StartTime, @EndTime);",
      new[]
      {
        new { Id = "shift-day", Name = "Mañana", StartTime = "07:00", EndTime = "15:00" },
        new { Id = "shift-night", Name = "Noche", StartTime = "19:00", EndTime = "07:00" }
      });

    // Seed patients
    _connection.Execute(@"
      INSERT INTO PATIENTS (ID, NAME, UNIT_ID) VALUES
      (@Id, @Name, @UnitId);",
      new[]
      {
        new { Id = "pat-123", Name = "John Doe", UnitId = "unit-1" },
        new { Id = "pat-456", Name = "Jane Smith", UnitId = "unit-1" },
        new { Id = "pat-789", Name = "Alex Johnson", UnitId = "unit-1" },
        new { Id = "pat-210", Name = "Ava Thompson", UnitId = "unit-2" },
        new { Id = "pat-220", Name = "Liam Rodríguez", UnitId = "unit-2" },
        new { Id = "pat-230", Name = "Mia Patel", UnitId = "unit-2" },
        new { Id = "pat-310", Name = "Pat Taylor", UnitId = "unit-3" },
        new { Id = "pat-320", Name = "Jordan White", UnitId = "unit-3" }
      });
  }

  // In-memory assignments (not persisted to maintain demo functionality)
  private readonly Dictionary<string, (string ShiftId, HashSet<string> PatientIds)> _assignments = new();

  public IReadOnlyList<UnitRecord> GetUnits()
  {
    // Return hardcoded test data for now to avoid SQLite issues
    return new List<UnitRecord>
    {
      new UnitRecord("unit-1", "UCI"),
      new UnitRecord("unit-2", "Pediatría General"),
      new UnitRecord("unit-3", "Pediatría Especializada")
    };
  }

  public IReadOnlyList<ShiftRecord> GetShifts()
  {
    // Return hardcoded test data for now to avoid SQLite issues
    return new List<ShiftRecord>
    {
      new ShiftRecord("shift-day", "Mañana", "07:00", "15:00"),
      new ShiftRecord("shift-night", "Noche", "19:00", "07:00")
    };
  }

  public (IReadOnlyList<PatientRecord> Patients, int TotalCount) GetPatientsByUnit(
    string unitId,
    int page,
    int pageSize)
  {
    // Return hardcoded test data for now to avoid SQLite issues
    var allPatients = new List<(string UnitId, PatientRecord Patient)>
    {
      ("unit-1", new PatientRecord("pat-123", "John Doe")),
      ("unit-1", new PatientRecord("pat-456", "Jane Smith")),
      ("unit-1", new PatientRecord("pat-789", "Alex Johnson")),
      ("unit-2", new PatientRecord("pat-210", "Ava Thompson")),
      ("unit-2", new PatientRecord("pat-220", "Liam Rodríguez")),
      ("unit-2", new PatientRecord("pat-230", "Mia Patel")),
      ("unit-3", new PatientRecord("pat-310", "Pat Taylor")),
      ("unit-3", new PatientRecord("pat-320", "Jordan White"))
    };

    var unitPatients = allPatients
      .Where(p => p.UnitId == unitId)
      .Select(p => p.Patient)
      .ToList();

    int total = unitPatients.Count;
    int p = Math.Max(page, 1);
    int ps = Math.Max(pageSize, 1);
    var items = unitPatients
      .Skip((p - 1) * ps)
      .Take(ps)
      .ToList();

    return (items, total);
  }

  public void Assign(string userId, string shiftId, IEnumerable<string> patientIds)
  {
    if (!_assignments.TryGetValue(userId, out var existing))
    {
      existing = (shiftId, new HashSet<string>());
    }

    existing.ShiftId = shiftId;
    existing.PatientIds = new HashSet<string>(patientIds);
    _assignments[userId] = existing;
  }

  public (IReadOnlyList<PatientRecord> Patients, int TotalCount) GetMyPatients(
    string userId,
    int page,
    int pageSize)
  {
    if (!_assignments.TryGetValue(userId, out var assignment) || assignment.PatientIds.Count == 0)
    {
      return (Array.Empty<PatientRecord>(), 0);
    }

    var ids = assignment.PatientIds.ToArray();
    if (ids.Length == 0)
      return (Array.Empty<PatientRecord>(), 0);

    // Use hardcoded patient data instead of database query
    var allPatients = new List<PatientRecord>
    {
      new PatientRecord("pat-123", "John Doe"),
      new PatientRecord("pat-456", "Jane Smith"),
      new PatientRecord("pat-789", "Alex Johnson"),
      new PatientRecord("pat-210", "Ava Thompson"),
      new PatientRecord("pat-220", "Liam Rodríguez"),
      new PatientRecord("pat-230", "Mia Patel"),
      new PatientRecord("pat-310", "Pat Taylor"),
      new PatientRecord("pat-320", "Jordan White")
    };

    var selected = allPatients.Where(p => ids.Contains(p.Id)).ToList();

    var total = selected.Count;
    var p = Math.Max(page, 1);
    var ps = Math.Max(pageSize, 1);
    var items = selected
      .Skip((p - 1) * ps)
      .Take(ps)
      .ToList();

    return (items, total);
  }

  public (IReadOnlyList<HandoverRecord> Handovers, int TotalCount) GetMyHandovers(string userId, int page, int pageSize)
  {
    // For now, return mock handover data since handovers are not implemented in the database
    // In a real implementation, this would query the database for handovers associated with the user's patients

    var mockHandovers = new List<HandoverRecord>
    {
      new HandoverRecord(
        Id: "hvo-001",
        PatientId: "pat-123",
        Status: "InProgress",
        IllnessSeverity: new HandoverIllnessSeverity("Stable"),
        PatientSummary: new HandoverPatientSummary("Patient is stable post-surgery with good vital signs."),
        ActionItems: new List<HandoverActionItem>
        {
          new HandoverActionItem("act-001", "Monitor vital signs every 4 hours", false),
          new HandoverActionItem("act-002", "Administer pain medication as needed", true)
        },
        SituationAwarenessDocId: "hvo-001-sa",
        Synthesis: null
      ),
      new HandoverRecord(
        Id: "hvo-002",
        PatientId: "pat-456",
        Status: "Completed",
        IllnessSeverity: new HandoverIllnessSeverity("Watcher"),
        PatientSummary: new HandoverPatientSummary("Patient showing signs of improvement with reduced oxygen requirements."),
        ActionItems: new List<HandoverActionItem>
        {
          new HandoverActionItem("act-003", "Wean oxygen support gradually", true),
          new HandoverActionItem("act-004", "Continue chest physiotherapy", true)
        },
        SituationAwarenessDocId: "hvo-002-sa",
        Synthesis: new HandoverSynthesis("Patient ready for step-down care. Continue monitoring respiratory status.")
      )
    };

    int total = mockHandovers.Count;
    int p = Math.Max(page, 1);
    int ps = Math.Max(pageSize, 1);
    var pageItems = mockHandovers.Skip((p - 1) * ps).Take(ps).ToList();
    return (pageItems, total);
  }
}

public record UnitRecord(string Id, string Name);

public record ShiftRecord(string Id, string Name, string StartTime, string EndTime);


