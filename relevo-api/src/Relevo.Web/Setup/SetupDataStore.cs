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
      );

      CREATE TABLE USER_ASSIGNMENTS (
        USER_ID TEXT NOT NULL,
        SHIFT_ID TEXT NOT NULL,
        PATIENT_ID TEXT NOT NULL,
        ASSIGNED_AT DATETIME DEFAULT CURRENT_TIMESTAMP,
        PRIMARY KEY (USER_ID, PATIENT_ID),
        FOREIGN KEY (PATIENT_ID) REFERENCES PATIENTS(ID),
        FOREIGN KEY (SHIFT_ID) REFERENCES SHIFTS(ID)
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

    // Seed patients - 35 patients distributed across units
    _connection.Execute(@"
      INSERT INTO PATIENTS (ID, NAME, UNIT_ID) VALUES
      (@Id, @Name, @UnitId);",
      new[]
      {
        // UCI (unit-1) - 12 patients
        new { Id = "pat-001", Name = "María García", UnitId = "unit-1" },
        new { Id = "pat-002", Name = "Carlos Rodríguez", UnitId = "unit-1" },
        new { Id = "pat-003", Name = "Ana López", UnitId = "unit-1" },
        new { Id = "pat-004", Name = "Miguel Hernández", UnitId = "unit-1" },
        new { Id = "pat-005", Name = "Isabella González", UnitId = "unit-1" },
        new { Id = "pat-006", Name = "David Pérez", UnitId = "unit-1" },
        new { Id = "pat-007", Name = "Sofia Martínez", UnitId = "unit-1" },
        new { Id = "pat-008", Name = "José Sánchez", UnitId = "unit-1" },
        new { Id = "pat-009", Name = "Carmen Díaz", UnitId = "unit-1" },
        new { Id = "pat-010", Name = "Antonio Moreno", UnitId = "unit-1" },
        new { Id = "pat-011", Name = "Elena Jiménez", UnitId = "unit-1" },
        new { Id = "pat-012", Name = "Francisco Ruiz", UnitId = "unit-1" },

        // Pediatría General (unit-2) - 12 patients
        new { Id = "pat-013", Name = "Lucía Álvarez", UnitId = "unit-2" },
        new { Id = "pat-014", Name = "Pablo Romero", UnitId = "unit-2" },
        new { Id = "pat-015", Name = "Valentina Navarro", UnitId = "unit-2" },
        new { Id = "pat-016", Name = "Diego Torres", UnitId = "unit-2" },
        new { Id = "pat-017", Name = "Marta Ramírez", UnitId = "unit-2" },
        new { Id = "pat-018", Name = "Adrián Gil", UnitId = "unit-2" },
        new { Id = "pat-019", Name = "Clara Serrano", UnitId = "unit-2" },
        new { Id = "pat-020", Name = "Hugo Castro", UnitId = "unit-2" },
        new { Id = "pat-021", Name = "Natalia Rubio", UnitId = "unit-2" },
        new { Id = "pat-022", Name = "Iván Ortega", UnitId = "unit-2" },
        new { Id = "pat-023", Name = "Paula Delgado", UnitId = "unit-2" },
        new { Id = "pat-024", Name = "Mario Guerrero", UnitId = "unit-2" },

        // Pediatría Especializada (unit-3) - 11 patients
        new { Id = "pat-025", Name = "Laura Flores", UnitId = "unit-3" },
        new { Id = "pat-026", Name = "Álvaro Vargas", UnitId = "unit-3" },
        new { Id = "pat-027", Name = "Cristina Medina", UnitId = "unit-3" },
        new { Id = "pat-028", Name = "Sergio Herrera", UnitId = "unit-3" },
        new { Id = "pat-029", Name = "Alicia Castro", UnitId = "unit-3" },
        new { Id = "pat-030", Name = "Roberto Vega", UnitId = "unit-3" },
        new { Id = "pat-031", Name = "Beatriz León", UnitId = "unit-3" },
        new { Id = "pat-032", Name = "Manuel Peña", UnitId = "unit-3" },
        new { Id = "pat-033", Name = "Silvia Cortés", UnitId = "unit-3" },
        new { Id = "pat-034", Name = "Fernando Aguilar", UnitId = "unit-3" },
        new { Id = "pat-035", Name = "Teresa Santana", UnitId = "unit-3" }
      });
  }

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
    // For tests, return hardcoded data since we're using SQLite
    var allPatients = new List<(string UnitId, PatientRecord Patient)>
    {
      // UCI (unit-1) - 12 patients
      ("unit-1", new PatientRecord("pat-001", "María García")),
      ("unit-1", new PatientRecord("pat-002", "Carlos Rodríguez")),
      ("unit-1", new PatientRecord("pat-003", "Ana López")),
      ("unit-1", new PatientRecord("pat-004", "Miguel Hernández")),
      ("unit-1", new PatientRecord("pat-005", "Isabella González")),
      ("unit-1", new PatientRecord("pat-006", "David Pérez")),
      ("unit-1", new PatientRecord("pat-007", "Sofia Martínez")),
      ("unit-1", new PatientRecord("pat-008", "José Sánchez")),
      ("unit-1", new PatientRecord("pat-009", "Carmen Díaz")),
      ("unit-1", new PatientRecord("pat-010", "Antonio Moreno")),
      ("unit-1", new PatientRecord("pat-011", "Elena Jiménez")),
      ("unit-1", new PatientRecord("pat-012", "Francisco Ruiz")),

      // Pediatría General (unit-2) - 12 patients
      ("unit-2", new PatientRecord("pat-013", "Lucía Álvarez")),
      ("unit-2", new PatientRecord("pat-014", "Pablo Romero")),
      ("unit-2", new PatientRecord("pat-015", "Valentina Navarro")),
      ("unit-2", new PatientRecord("pat-016", "Diego Torres")),
      ("unit-2", new PatientRecord("pat-017", "Marta Ramírez")),
      ("unit-2", new PatientRecord("pat-018", "Adrián Gil")),
      ("unit-2", new PatientRecord("pat-019", "Clara Serrano")),
      ("unit-2", new PatientRecord("pat-020", "Hugo Castro")),
      ("unit-2", new PatientRecord("pat-021", "Natalia Rubio")),
      ("unit-2", new PatientRecord("pat-022", "Iván Ortega")),
      ("unit-2", new PatientRecord("pat-023", "Paula Delgado")),
      ("unit-2", new PatientRecord("pat-024", "Mario Guerrero")),

      // Pediatría Especializada (unit-3) - 11 patients
      ("unit-3", new PatientRecord("pat-025", "Laura Flores")),
      ("unit-3", new PatientRecord("pat-026", "Álvaro Vargas")),
      ("unit-3", new PatientRecord("pat-027", "Cristina Medina")),
      ("unit-3", new PatientRecord("pat-028", "Sergio Herrera")),
      ("unit-3", new PatientRecord("pat-029", "Alicia Castro")),
      ("unit-3", new PatientRecord("pat-030", "Roberto Vega")),
      ("unit-3", new PatientRecord("pat-031", "Beatriz León")),
      ("unit-3", new PatientRecord("pat-032", "Manuel Peña")),
      ("unit-3", new PatientRecord("pat-033", "Silvia Cortés")),
      ("unit-3", new PatientRecord("pat-034", "Fernando Aguilar")),
      ("unit-3", new PatientRecord("pat-035", "Teresa Santana"))
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
    // Remove existing assignments for this user
    _connection.Execute("DELETE FROM USER_ASSIGNMENTS WHERE USER_ID = @UserId",
        new { UserId = userId });

    // Insert new assignments
    foreach (var patientId in patientIds)
    {
      _connection.Execute(@"
        INSERT INTO USER_ASSIGNMENTS (USER_ID, SHIFT_ID, PATIENT_ID)
        VALUES (@UserId, @ShiftId, @PatientId)",
        new { UserId = userId, ShiftId = shiftId, PatientId = patientId });
    }
  }

  public (IReadOnlyList<PatientRecord> Patients, int TotalCount) GetMyPatients(
    string userId,
    int page,
    int pageSize)
  {
    // Get total count of assigned patients
    var total = _connection.ExecuteScalar<int>(
      "SELECT COUNT(*) FROM USER_ASSIGNMENTS WHERE USER_ID = @UserId",
      new { UserId = userId });

    if (total == 0)
      return (Array.Empty<PatientRecord>(), 0);

    // Get assigned patients with pagination
    var p = Math.Max(page, 1);
    var ps = Math.Max(pageSize, 1);
    var offset = (p - 1) * ps;

    var patients = _connection.Query<PatientRecord>(@"
      SELECT p.ID, p.NAME
      FROM PATIENTS p
      INNER JOIN USER_ASSIGNMENTS ua ON p.ID = ua.PATIENT_ID
      WHERE ua.USER_ID = @UserId
      ORDER BY p.ID
      LIMIT @PageSize OFFSET @Offset",
      new { UserId = userId, PageSize = ps, Offset = offset });

    return (patients.ToList(), total);
  }

  public (IReadOnlyList<HandoverRecord> Handovers, int TotalCount) GetMyHandovers(string userId, int page, int pageSize)
  {
    // For now, return mock handover data since handovers are not implemented in the database
    // In a real implementation, this would query the database for handovers associated with the user's patients

    var mockHandovers = new List<HandoverRecord>
    {
      new HandoverRecord(
        Id: "hvo-001",
        PatientId: "pat-001",
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
        PatientId: "pat-013",
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
      ),
      new HandoverRecord(
        Id: "hvo-003",
        PatientId: "pat-025",
        Status: "InProgress",
        IllnessSeverity: new HandoverIllnessSeverity("Unstable"),
        PatientSummary: new HandoverPatientSummary("Patient requires close monitoring due to fluctuating vital signs."),
        ActionItems: new List<HandoverActionItem>
        {
          new HandoverActionItem("act-005", "Continuous vital signs monitoring", false),
          new HandoverActionItem("act-006", "Prepare emergency medications", false)
        },
        SituationAwarenessDocId: "hvo-003-sa",
        Synthesis: null
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


