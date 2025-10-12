using Relevo.Web.Patients;
using Relevo.Web.Me;
using System.Data;
using Dapper;
using Oracle.ManagedDataAccess.Client;
using Relevo.Core.Interfaces;

// Use specific types from Core layer to avoid conflicts
using PatientRecord = Relevo.Core.Interfaces.PatientRecord;
using UnitRecord = Relevo.Core.Interfaces.UnitRecord;
using ShiftRecord = Relevo.Core.Interfaces.ShiftRecord;
using HandoverRecord = Relevo.Core.Interfaces.HandoverRecord;

namespace Relevo.Web.Setup;

public class SetupDataStore : ISetupDataProvider
{
  private readonly IDbConnection _connection;

  public SetupDataStore()
  {
    // Use Oracle database for testing
    _connection = new OracleConnection("User Id=RELEVO_APP;Password=TuPass123;Data Source=localhost:1521/XE;Pooling=true;Connection Timeout=15");
    _connection.Open();

    // Database tables are created by SQL scripts on container startup
    // Just seed test data into existing tables
    SeedTestData();
  }


  private void SeedTestData()
  {
    using var transaction = _connection.BeginTransaction();
    try
    {
      // Clear existing test data first
      _connection.Execute("DELETE FROM USER_ASSIGNMENTS", transaction: transaction);
      _connection.Execute("DELETE FROM PATIENTS", transaction: transaction);
      _connection.Execute("DELETE FROM SHIFTS", transaction: transaction);
      _connection.Execute("DELETE FROM UNITS", transaction: transaction);
      _connection.Execute("DELETE FROM CONTRIBUTORS", transaction: transaction);

      // Seed units
      _connection.Execute(@"
        INSERT INTO UNITS (ID, NAME, DESCRIPTION, CREATED_AT, UPDATED_AT) VALUES
        (:Id, :Name, :Description, SYSTIMESTAMP, SYSTIMESTAMP)",
        new { Id = "unit-1", Name = "UCI", Description = "Unidad de Cuidados Intensivos" }, transaction);

      _connection.Execute(@"
        INSERT INTO UNITS (ID, NAME, DESCRIPTION, CREATED_AT, UPDATED_AT) VALUES
        (:Id, :Name, :Description, SYSTIMESTAMP, SYSTIMESTAMP)",
        new { Id = "unit-2", Name = "Pediatría General", Description = "Pediatría General" }, transaction);

      _connection.Execute(@"
        INSERT INTO UNITS (ID, NAME, DESCRIPTION, CREATED_AT, UPDATED_AT) VALUES
        (:Id, :Name, :Description, SYSTIMESTAMP, SYSTIMESTAMP)",
        new { Id = "unit-3", Name = "Pediatría Especializada", Description = "Pediatría Especializada" }, transaction);

      // Seed shifts
      _connection.Execute(@"
        INSERT INTO SHIFTS (ID, NAME, START_TIME, END_TIME, CREATED_AT, UPDATED_AT) VALUES
        (:Id, :Name, :StartTime, :EndTime, SYSTIMESTAMP, SYSTIMESTAMP)",
        new { Id = "shift-day", Name = "Mañana", StartTime = "07:00", EndTime = "15:00" }, transaction);

      _connection.Execute(@"
        INSERT INTO SHIFTS (ID, NAME, START_TIME, END_TIME, CREATED_AT, UPDATED_AT) VALUES
        (:Id, :Name, :StartTime, :EndTime, SYSTIMESTAMP, SYSTIMESTAMP)",
        new { Id = "shift-night", Name = "Noche", StartTime = "19:00", EndTime = "07:00" }, transaction);

      // Seed contributors for existing functionality
      _connection.Execute(@"
        INSERT INTO CONTRIBUTORS (ID, NAME, EMAIL, PHONE_NUMBER, CREATED_AT, UPDATED_AT) VALUES
        (1, 'Ardalis', 'ardalis@example.com', '+1-555-0101', SYSTIMESTAMP, SYSTIMESTAMP)", transaction: transaction);

      // Seed a few patients
      _connection.Execute(@"
        INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, CREATED_AT, UPDATED_AT) VALUES
        (:Id, :Name, :UnitId, :DateOfBirth, :Gender, :AdmissionDate, :RoomNumber, :Diagnosis, SYSTIMESTAMP, SYSTIMESTAMP)",
        new { Id = "pat-001", Name = "María García", UnitId = "unit-1", DateOfBirth = new DateTime(2010, 1, 1), Gender = "Female", AdmissionDate = DateTime.Now.AddDays(-2), RoomNumber = "101", Diagnosis = "Neumonía" }, transaction);

      _connection.Execute(@"
        INSERT INTO PATIENTS (ID, NAME, UNIT_ID, DATE_OF_BIRTH, GENDER, ADMISSION_DATE, ROOM_NUMBER, DIAGNOSIS, CREATED_AT, UPDATED_AT) VALUES
        (:Id, :Name, :UnitId, :DateOfBirth, :Gender, :AdmissionDate, :RoomNumber, :Diagnosis, SYSTIMESTAMP, SYSTIMESTAMP)",
        new { Id = "pat-002", Name = "Carlos Rodríguez", UnitId = "unit-2", DateOfBirth = new DateTime(2012, 5, 15), Gender = "Male", AdmissionDate = DateTime.Now.AddDays(-1), RoomNumber = "201", Diagnosis = "Gastroenteritis" }, transaction);

      transaction.Commit();
    }
    catch (Exception ex)
    {
      transaction.Rollback();
      // Log error but don't fail - tests should work with whatever data exists
      Console.WriteLine($"Error seeding test data: {ex.Message}");
    }
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
      ("unit-1", new PatientRecord("pat-001", "María García", "not-started", null, 14m, "201", "Neumonía adquirida en comunidad", null, null)),
      ("unit-1", new PatientRecord("pat-002", "Carlos Rodríguez", "not-started", null, 16m, "202", "Sepsis secundaria a infección urinaria", null, null)),
      ("unit-1", new PatientRecord("pat-003", "Ana López", "not-started", null, 12m, "203", "Estado asmático agudo", null, null)),
      ("unit-1", new PatientRecord("pat-004", "Miguel Hernández", "not-started", null, 15m, "204", "Trauma craneoencefálico moderado", null, null)),
      ("unit-1", new PatientRecord("pat-005", "Isabella González", "not-started", null, 13m, "205", "Insuficiencia respiratoria aguda", null, null)),
      ("unit-1", new PatientRecord("pat-006", "David Pérez", "not-started", null, 11m, "206", "Choque séptico", null, null)),
      ("unit-1", new PatientRecord("pat-007", "Sofia Martínez", "not-started", null, 17m, "207", "Meningitis bacteriana", null, null)),
      ("unit-1", new PatientRecord("pat-008", "José Sánchez", "not-started", null, 12m, "208", "Quemaduras de segundo grado", null, null)),
      ("unit-1", new PatientRecord("pat-009", "Carmen Díaz", "not-started", null, 14m, "209", "Convulsiones febriles", null, null)),
      ("unit-1", new PatientRecord("pat-010", "Antonio Moreno", "not-started", null, 18m, "210", "Intoxicación medicamentosa", null, null)),
      ("unit-1", new PatientRecord("pat-011", "Elena Jiménez", "not-started", null, 13m, "211", "Hipoglucemia severa", null, null)),
      ("unit-1", new PatientRecord("pat-012", "Francisco Ruiz", "not-started", null, 12m, "212", "Trauma abdominal", null, null)),

      // Pediatría General (unit-2) - 12 patients
      ("unit-2", new PatientRecord("pat-013", "Lucía Álvarez", "not-started", null, 9m, "301", "Bronquiolitis", null, null)),
      ("unit-2", new PatientRecord("pat-014", "Pablo Romero", "not-started", null, 11m, "302", "Gastroenteritis aguda", null, null)),
      ("unit-2", new PatientRecord("pat-015", "Valentina Navarro", "not-started", null, 10m, "303", "Otitis media aguda", null, null)),
      ("unit-2", new PatientRecord("pat-016", "Diego Torres", "not-started", null, 12m, "304", "Neumonía adquirida en comunidad", null, null)),
      ("unit-2", new PatientRecord("pat-017", "Marta Ramírez", "not-started", null, 9m, "305", "Infección urinaria", null, null)),
      ("unit-2", new PatientRecord("pat-018", "Adrián Gil", "not-started", null, 12m, "306", "Fractura de antebrazo", null, null)),
      ("unit-2", new PatientRecord("pat-019", "Clara Serrano", "not-started", null, 11m, "307", "Varicela", null, null)),
      ("unit-2", new PatientRecord("pat-020", "Hugo Castro", "not-started", null, 7m, "308", "Deshidratación moderada", null, null)),
      ("unit-2", new PatientRecord("pat-021", "Natalia Rubio", "not-started", null, 10m, "309", "Apendicitis aguda", null, null)),
      ("unit-2", new PatientRecord("pat-022", "Iván Ortega", "not-started", null, 13m, "310", "Asma agudizada", null, null)),
      ("unit-2", new PatientRecord("pat-023", "Paula Delgado", "not-started", null, 11m, "311", "Faringoamigdalitis", null, null)),
      ("unit-2", new PatientRecord("pat-024", "Mario Guerrero", "not-started", null, 10m, "312", "Traumatismo craneoencefálico leve", null, null)),

      // Pediatría Especializada (unit-3) - 11 patients
      ("unit-3", new PatientRecord("pat-025", "Laura Flores", "not-started", null, 14m, "401", "Cardiopatía congénita", null, null)),
      ("unit-3", new PatientRecord("pat-026", "Álvaro Vargas", "not-started", null, 13m, "402", "Diabetes mellitus tipo 1", null, null)),
      ("unit-3", new PatientRecord("pat-027", "Cristina Medina", "not-started", null, 16m, "403", "Fibrosis quística", null, null)),
      ("unit-3", new PatientRecord("pat-028", "Sergio Herrera", "not-started", null, 12m, "404", "Trastorno del espectro autista", null, null)),
      ("unit-3", new PatientRecord("pat-029", "Alicia Castro", "not-started", null, 11m, "405", "Epilepsia refractaria", null, null)),
      ("unit-3", new PatientRecord("pat-030", "Roberto Vega", "not-started", null, 15m, "406", "Leucemia linfoblástica aguda", null, null)),
      ("unit-3", new PatientRecord("pat-031", "Beatriz León", "not-started", null, 10m, "407", "Síndrome de Down con cardiopatía", null, null)),
      ("unit-3", new PatientRecord("pat-032", "Manuel Peña", "not-started", null, 17m, "408", "Parálisis cerebral", null, null)),
      ("unit-3", new PatientRecord("pat-033", "Silvia Cortés", "not-started", null, 9m, "409", "Prematuridad extrema", null, null)),
      ("unit-3", new PatientRecord("pat-034", "Fernando Aguilar", "not-started", null, 13m, "410", "Trastorno de déficit de atención", null, null)),
      ("unit-3", new PatientRecord("pat-035", "Teresa Santana", "not-started", null, 14m, "411", "Talasemia mayor", null, null))
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
    _connection.Execute("DELETE FROM USER_ASSIGNMENTS WHERE USER_ID = :UserId",
        new { UserId = userId });

    // Insert new assignments
    foreach (var patientId in patientIds)
    {
      _connection.Execute(@"
        INSERT INTO USER_ASSIGNMENTS (USER_ID, SHIFT_ID, PATIENT_ID)
        VALUES (:UserId, :ShiftId, :PatientId)",
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
      "SELECT COUNT(*) FROM USER_ASSIGNMENTS WHERE USER_ID = :UserId",
      new { UserId = userId });

    if (total == 0)
      return (Array.Empty<PatientRecord>(), 0);

    // Get assigned patients with pagination
    var p = Math.Max(page, 1);
    var ps = Math.Max(pageSize, 1);
    var offset = (p - 1) * ps;

    var patients = _connection.Query<PatientRecord>(@"
      SELECT ID, NAME FROM (
        SELECT p.ID, p.NAME, ROW_NUMBER() OVER (ORDER BY p.ID) AS RN
        FROM PATIENTS p
        INNER JOIN USER_ASSIGNMENTS ua ON p.ID = ua.PATIENT_ID
        WHERE ua.USER_ID = :UserId
      ) WHERE RN BETWEEN :StartRow AND :EndRow",
      new { UserId = userId, StartRow = offset + 1, EndRow = offset + ps });

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
        AssignmentId: "assign-001",
        PatientId: "pat-001",
        PatientName: "María García",
        Status: "InProgress",
        IllnessSeverity: new HandoverIllnessSeverity("Stable"),
        PatientSummary: new HandoverPatientSummary("Patient is stable post-surgery with good vital signs."),
        SituationAwarenessDocId: "hvo-001-sa",
        Synthesis: null,
        ShiftName: "Mañana",
        CreatedBy: "user-123",
        AssignedTo: "user-123",
        CreatedByName: null,
        AssignedToName: null,
        ReceiverUserId: null,
        ResponsiblePhysicianId: "user-123",
        ResponsiblePhysicianName: "Dr. García",
        CreatedAt: DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
        ReadyAt: null,
        StartedAt: null,
        AcknowledgedAt: null,
        AcceptedAt: null,
        CompletedAt: null,
        CancelledAt: null,
        RejectedAt: null,
        RejectionReason: null,
        ExpiredAt: null,
        HandoverType: null,
        HandoverWindowDate: DateTime.Now.Date,
        FromShiftId: "shift-day",
        ToShiftId: "shift-night",
        ToDoctorId: "user-123",
        StateName: "Draft",
        Version: 1
      ),
      new HandoverRecord(
        Id: "hvo-002",
        AssignmentId: "assign-002",
        PatientId: "pat-013",
        PatientName: "Lucía Álvarez",
        Status: "Completed",
        IllnessSeverity: new HandoverIllnessSeverity("Watcher"),
        PatientSummary: new HandoverPatientSummary("Patient showing signs of improvement with reduced oxygen requirements."),
        SituationAwarenessDocId: "hvo-002-sa",
        Synthesis: new HandoverSynthesis("Patient ready for step-down care. Continue monitoring respiratory status."),
        ShiftName: "Noche",
        CreatedBy: "user-123",
        AssignedTo: "user-123",
        CreatedByName: null,
        AssignedToName: null,
        ReceiverUserId: null,
        ResponsiblePhysicianId: "user-123",
        ResponsiblePhysicianName: "Dr. García",
        CreatedAt: DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
        ReadyAt: null,
        StartedAt: null,
        AcknowledgedAt: null,
        AcceptedAt: null,
        CompletedAt: DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
        CancelledAt: null,
        RejectedAt: null,
        RejectionReason: null,
        ExpiredAt: null,
        HandoverType: null,
        HandoverWindowDate: DateTime.Now.Date,
        FromShiftId: "shift-night",
        ToShiftId: "shift-day",
        ToDoctorId: "user-123",
        StateName: "Draft",
        Version: 1
      ),
      new HandoverRecord(
        Id: "hvo-003",
        AssignmentId: "assign-003",
        PatientId: "pat-025",
        PatientName: "Laura Flores",
        Status: "InProgress",
        IllnessSeverity: new HandoverIllnessSeverity("Unstable"),
        PatientSummary: new HandoverPatientSummary("Patient requires close monitoring due to fluctuating vital signs."),
        SituationAwarenessDocId: "hvo-003-sa",
        Synthesis: null,
        ShiftName: "Mañana",
        CreatedBy: "user-123",
        AssignedTo: "user-123",
        CreatedByName: null,
        AssignedToName: null,
        ReceiverUserId: null,
        ResponsiblePhysicianId: "user-123",
        ResponsiblePhysicianName: "Dr. García",
        CreatedAt: DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
        ReadyAt: null,
        StartedAt: null,
        AcknowledgedAt: null,
        AcceptedAt: null,
        CompletedAt: null,
        CancelledAt: null,
        RejectedAt: null,
        RejectionReason: null,
        ExpiredAt: null,
        HandoverType: null,
        HandoverWindowDate: DateTime.Now.Date,
        FromShiftId: "shift-day",
        ToShiftId: "shift-night",
        ToDoctorId: "user-123",
        StateName: "Draft",
        Version: 1
      )
    };

    int total = mockHandovers.Count;
    int p = Math.Max(page, 1);
    int ps = Math.Max(pageSize, 1);
    var pageItems = mockHandovers.Skip((p - 1) * ps).Take(ps).ToList();
    return (pageItems, total);
  }

  // Handover Creation and Management
  public async Task<HandoverRecord> CreateHandoverAsync(CreateHandoverRequest request)
  {
    await Task.CompletedTask; // Make async
    throw new NotImplementedException("CreateHandoverAsync not implemented in test data store");
  }

  public async Task<bool> StartHandoverAsync(string handoverId, string userId)
  {
    await Task.CompletedTask; // Make async
    throw new NotImplementedException("StartHandoverAsync not implemented in test data store");
  }

  public async Task<bool> AcceptHandoverAsync(string handoverId, string userId)
  {
    await Task.CompletedTask; // Make async
    throw new NotImplementedException("AcceptHandoverAsync not implemented in test data store");
  }

  public async Task<bool> CompleteHandoverAsync(string handoverId, string userId)
  {
    await Task.CompletedTask; // Make async
    throw new NotImplementedException("CompleteHandoverAsync not implemented in test data store");
  }

  public async Task<IReadOnlyList<HandoverRecord>> GetPendingHandoversForUserAsync(string userId)
  {
    await Task.CompletedTask; // Make async
    // Return mock pending handovers for testing
    return new List<HandoverRecord>
    {
      new HandoverRecord(
        Id: "pending-hvo-001",
        AssignmentId: "assign-001",
        PatientId: "pat-001",
        PatientName: "María García",
        Status: "Ready",
        IllnessSeverity: new HandoverIllnessSeverity("Stable"),
        PatientSummary: new HandoverPatientSummary("Patient awaiting handover acceptance."),
        SituationAwarenessDocId: null,
        Synthesis: null,
        ShiftName: "Mañana → Noche",
        CreatedBy: "user-demo12345678901234567890123456",
        AssignedTo: userId,
        CreatedByName: null,
        AssignedToName: null,
        ReceiverUserId: userId,
        ResponsiblePhysicianId: "user-123",
        ResponsiblePhysicianName: "Dr. García",
        CreatedAt: DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
        ReadyAt: null,
        StartedAt: null,
        AcknowledgedAt: null,
        AcceptedAt: null,
        CompletedAt: null,
        CancelledAt: null,
        RejectedAt: null,
        RejectionReason: null,
        ExpiredAt: null,
        HandoverType: null,
        HandoverWindowDate: DateTime.Now.Date,
        FromShiftId: "shift-day",
        ToShiftId: "shift-night",
        ToDoctorId: userId,
        StateName: "Draft",
        Version: 1
      )
    };
  }

  public async Task<IReadOnlyList<HandoverRecord>> GetHandoversByPatientAsync(string patientId)
  {
    await Task.CompletedTask; // Make async
    // Return mock handovers for the patient
    return new List<HandoverRecord>
    {
      new HandoverRecord(
        Id: "patient-hvo-001",
        AssignmentId: "assign-001",
        PatientId: patientId,
        PatientName: "Patient Name",
        Status: "Completed",
        IllnessSeverity: new HandoverIllnessSeverity("Stable"),
        PatientSummary: new HandoverPatientSummary("Patient handover completed."),
        SituationAwarenessDocId: null,
        Synthesis: new HandoverSynthesis("Handover completed successfully."),
        ShiftName: "Mañana → Noche",
        CreatedBy: "user-123",
        AssignedTo: "user-456",
        CreatedByName: null,
        AssignedToName: null,
        ReceiverUserId: "user-456",
        ResponsiblePhysicianId: "user-123",
        ResponsiblePhysicianName: "Dr. García",
        CreatedAt: DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
        ReadyAt: null,
        StartedAt: null,
        AcknowledgedAt: null,
        AcceptedAt: null,
        CompletedAt: DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
        CancelledAt: null,
        RejectedAt: null,
        RejectionReason: null,
        ExpiredAt: null,
        HandoverType: null,
        HandoverWindowDate: DateTime.Now.Date,
        FromShiftId: "shift-day",
        ToShiftId: "shift-night",
        ToDoctorId: "user-456",
        StateName: "Draft",
        Version: 1
      )
    };
  }

  public async Task<IReadOnlyList<HandoverRecord>> GetShiftTransitionHandoversAsync(string fromDoctorId, string toDoctorId)
  {
    await Task.CompletedTask; // Make async
    // Return mock shift transition handovers
    return new List<HandoverRecord>
    {
      new HandoverRecord(
        Id: "transition-hvo-001",
        AssignmentId: "assign-001",
        PatientId: "pat-001",
        PatientName: "María García",
        Status: "Completed",
        IllnessSeverity: new HandoverIllnessSeverity("Stable"),
        PatientSummary: new HandoverPatientSummary("Shift transition handover completed."),
        SituationAwarenessDocId: null,
        Synthesis: new HandoverSynthesis("Successful shift transition."),
        ShiftName: "Mañana → Noche",
        CreatedBy: fromDoctorId,
        AssignedTo: toDoctorId,
        CreatedByName: null,
        AssignedToName: null,
        ReceiverUserId: toDoctorId,
        ResponsiblePhysicianId: fromDoctorId,
        ResponsiblePhysicianName: "Dr. García",
        CreatedAt: DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
        ReadyAt: null,
        StartedAt: null,
        AcknowledgedAt: null,
        AcceptedAt: null,
        CompletedAt: DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
        CancelledAt: null,
        RejectedAt: null,
        RejectionReason: null,
        ExpiredAt: null,
        HandoverType: null,
        HandoverWindowDate: DateTime.Now.Date,
        FromShiftId: "shift-day",
        ToShiftId: "shift-night",
        ToDoctorId: toDoctorId,
        StateName: "Draft",
        Version: 1
      )
    };
  }
}

public record UnitRecord(string Id, string Name);

public record ShiftRecord(string Id, string Name, string StartTime, string EndTime);


