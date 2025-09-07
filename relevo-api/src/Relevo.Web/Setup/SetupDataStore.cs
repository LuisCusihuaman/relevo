using Relevo.Web.Patients;
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
    const string sql = "SELECT ID AS Id, NAME AS Name FROM UNITS ORDER BY ID";
    return _connection.Query<UnitRecord>(sql).ToList();
  }

  public IReadOnlyList<ShiftRecord> GetShifts()
  {
    const string sql = "SELECT ID AS Id, NAME AS Name, START_TIME AS StartTime, END_TIME AS EndTime FROM SHIFTS ORDER BY ID";
    return _connection.Query<ShiftRecord>(sql).ToList();
  }

  public (IReadOnlyList<PatientRecord> Patients, int TotalCount) GetPatientsByUnit(
    string unitId,
    int page,
    int pageSize)
  {
    int p = Math.Max(page, 1);
    int ps = Math.Max(pageSize, 1);

    const string countSql = "SELECT COUNT(*) FROM PATIENTS WHERE UNIT_ID = @unitId";
    const string pageSql = @"
      SELECT ID AS Id, NAME AS Name
      FROM PATIENTS
      WHERE UNIT_ID = @unitId
      ORDER BY ID
      LIMIT @pageSize OFFSET @offset";

    int total = _connection.ExecuteScalar<int>(countSql, new { unitId });
    int offset = (p - 1) * ps;
    var items = _connection.Query<PatientRecord>(pageSql, new { unitId, pageSize = ps, offset }).ToList();

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

    // Query patients by IDs from database
    const string sql = "SELECT ID AS Id, NAME AS Name FROM PATIENTS WHERE ID IN @ids ORDER BY ID";
    var selected = _connection.Query<PatientRecord>(sql, new { ids }).ToList();

    var total = selected.Count;
    var p = Math.Max(page, 1);
    var ps = Math.Max(pageSize, 1);
    var items = selected
      .Skip((p - 1) * ps)
      .Take(ps)
      .ToList();

    return (items, total);
  }
}

public record UnitRecord(string Id, string Name);

public record ShiftRecord(string Id, string Name, string StartTime, string EndTime);


