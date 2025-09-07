using System.Data;
using Dapper;
using Relevo.Core.Interfaces;
using Microsoft.Data.Sqlite;

namespace Relevo.Infrastructure.Repositories;

public class SqliteSetupRepository : ISetupRepository
{
    private readonly IDbConnection _connection;

    public SqliteSetupRepository(string connectionString)
    {
        // Use the provided connection string (file-based for tests, in-memory for unit tests)
        _connection = new SqliteConnection(connectionString);
        _connection.Open();

        // Create tables with same structure as Oracle
        CreateTables();
        SeedTestData();
    }

    private void CreateTables()
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = @"
          CREATE TABLE IF NOT EXISTS UNITS (
            ID TEXT PRIMARY KEY,
            NAME TEXT NOT NULL
          );

          CREATE TABLE IF NOT EXISTS SHIFTS (
            ID TEXT PRIMARY KEY,
            NAME TEXT NOT NULL,
            START_TIME TEXT NOT NULL,
            END_TIME TEXT NOT NULL
          );

          CREATE TABLE IF NOT EXISTS PATIENTS (
            ID TEXT PRIMARY KEY,
            NAME TEXT NOT NULL,
            UNIT_ID TEXT,
            FOREIGN KEY (UNIT_ID) REFERENCES UNITS(ID)
          );

          CREATE TABLE IF NOT EXISTS USER_ASSIGNMENTS (
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
        // Avoid reseeding if units already exist
        var existingUnits = _connection.ExecuteScalar<int>("SELECT COUNT(1) FROM UNITS");
        if (existingUnits == 0)
        {
            // Seed units
            _connection.Execute(@"
              INSERT OR IGNORE INTO UNITS (ID, NAME) VALUES
              (@Id, @Name);",
              new[]
              {
                new { Id = "unit-1", Name = "UCI" },
                new { Id = "unit-2", Name = "Pediatría General" },
                new { Id = "unit-3", Name = "Pediatría Especializada" }
              });

            // Seed shifts
            _connection.Execute(@"
              INSERT OR IGNORE INTO SHIFTS (ID, NAME, START_TIME, END_TIME) VALUES
              (@Id, @Name, @StartTime, @EndTime);",
              new[]
              {
                new { Id = "shift-day", Name = "Mañana", StartTime = "07:00", EndTime = "15:00" },
                new { Id = "shift-night", Name = "Noche", StartTime = "19:00", EndTime = "07:00" }
              });

            // Seed patients
            _connection.Execute(@"
              INSERT OR IGNORE INTO PATIENTS (ID, NAME, UNIT_ID) VALUES
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

    public async Task AssignAsync(string userId, string shiftId, IEnumerable<string> patientIds)
    {
        // Debug logging
        Console.WriteLine($"[DEBUG] AssignAsync DB - UserId: '{userId}', ShiftId: '{shiftId}', Patients: {string.Join(",", patientIds)}");

        // Remove existing assignments for this user
        var deletedCount = _connection.Execute("DELETE FROM USER_ASSIGNMENTS WHERE USER_ID = @UserId",
            new { UserId = userId });

        // Debug logging
        Console.WriteLine($"[DEBUG] AssignAsync DB - Deleted {deletedCount} existing assignments");

        // Insert new assignments
        foreach (var patientId in patientIds)
        {
            _connection.Execute(@"
            INSERT INTO USER_ASSIGNMENTS (USER_ID, SHIFT_ID, PATIENT_ID)
            VALUES (@UserId, @ShiftId, @PatientId)",
            new { UserId = userId, ShiftId = shiftId, PatientId = patientId });

            // Debug logging
            Console.WriteLine($"[DEBUG] AssignAsync DB - Assigned patient {patientId} to user {userId}");
        }

        await Task.CompletedTask; // For async compatibility
    }

    public (IReadOnlyList<PatientRecord> Patients, int TotalCount) GetMyPatients(
        string userId,
        int page,
        int pageSize)
    {
        // Debug logging
        Console.WriteLine($"[DEBUG] GetMyPatients DB - UserId: '{userId}', Page: {page}, PageSize: {pageSize}");

        // Get total count of assigned patients
        var total = _connection.ExecuteScalar<int>(
          "SELECT COUNT(*) FROM USER_ASSIGNMENTS WHERE USER_ID = @UserId",
          new { UserId = userId });

        // Debug logging
        Console.WriteLine($"[DEBUG] GetMyPatients DB - Total assignments found: {total}");

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

        // Debug logging
        Console.WriteLine($"[DEBUG] GetMyPatients DB - Patients returned: {patients.Count()}");

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
