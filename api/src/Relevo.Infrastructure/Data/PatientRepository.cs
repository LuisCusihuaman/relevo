using System.Data;
using Dapper;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;

namespace Relevo.Infrastructure.Data;

public class PatientRepository(DapperConnectionFactory _connectionFactory) : IPatientRepository
{
  public async Task<(IReadOnlyList<PatientRecord> Patients, int TotalCount)> GetPatientsByUnitAsync(string unitId, int page, int pageSize)
  {
    using var conn = _connectionFactory.CreateConnection();

    var p = Math.Max(page, 1);
    var ps = Math.Max(pageSize, 1);
    var offset = (p - 1) * ps;

    // Count
    var total = await conn.ExecuteScalarAsync<int>(
        "SELECT COUNT(*) FROM PATIENTS WHERE UNIT_ID = :UnitId",
        new { UnitId = unitId });

    if (total == 0)
        return (Array.Empty<PatientRecord>(), 0);

    // Query
    const string sql = @"
      SELECT ID, NAME, HandoverStatus, HandoverId, Age, Room, DIAGNOSIS, Status, Severity FROM (
        SELECT
            p.ID,
            p.NAME,
            'not-started' as HandoverStatus,
            CAST(NULL AS VARCHAR2(50)) as HandoverId,
            ROUND(MONTHS_BETWEEN(SYSDATE, p.DATE_OF_BIRTH)/12, 1) as Age,
            p.ROOM_NUMBER as Room,
            p.DIAGNOSIS,
            CAST(NULL AS VARCHAR2(20)) as Status,
            CAST(NULL AS VARCHAR2(20)) as Severity,
            ROW_NUMBER() OVER (ORDER BY p.NAME) AS RN
        FROM PATIENTS p
        WHERE p.UNIT_ID = :UnitId
      )
      WHERE RN BETWEEN :StartRow AND :EndRow";

    var patients = await conn.QueryAsync<PatientRecord>(sql, new { UnitId = unitId, StartRow = offset + 1, EndRow = offset + ps });

    return (patients.ToList(), total);
  }

    public async Task<(IReadOnlyList<PatientRecord> Patients, int TotalCount)> GetAllPatientsAsync(int page, int pageSize)
    {
      using var conn = _connectionFactory.CreateConnection();

      var p = Math.Max(page, 1);
      var ps = Math.Max(pageSize, 1);
      var offset = (p - 1) * ps;

      // Count
      var total = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM PATIENTS");

      if (total == 0)
          return (Array.Empty<PatientRecord>(), 0);

      // Query
      const string sql = @"
        SELECT ID, NAME, HandoverStatus, HandoverId, Age, Room, DIAGNOSIS, Status, Severity FROM (
          SELECT
              p.ID,
              p.NAME,
              'not-started' as HandoverStatus,
              CAST(NULL AS VARCHAR2(50)) as HandoverId,
              ROUND(MONTHS_BETWEEN(SYSDATE, p.DATE_OF_BIRTH)/12, 1) as Age,
              p.ROOM_NUMBER as Room,
              p.DIAGNOSIS,
              CAST(NULL AS VARCHAR2(20)) as Status,
              CAST(NULL AS VARCHAR2(20)) as Severity,
              ROW_NUMBER() OVER (ORDER BY p.NAME) AS RN
          FROM PATIENTS p
        )
        WHERE RN BETWEEN :StartRow AND :EndRow";

      var patients = await conn.QueryAsync<PatientRecord>(sql, new { StartRow = offset + 1, EndRow = offset + ps });

      return (patients.ToList(), total);
    }

    public async Task<PatientSummaryRecord?> GetPatientSummaryAsync(string patientId)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT * FROM (
                SELECT ID,
                       PATIENT_ID as PatientId,
                       PHYSICIAN_ID as PhysicianId,
                       SUMMARY_TEXT as SummaryText,
                       CREATED_AT as CreatedAt,
                       UPDATED_AT as UpdatedAt,
                       LAST_EDITED_BY as LastEditedBy
                FROM PATIENT_SUMMARIES
                WHERE PATIENT_ID = :PatientId
                ORDER BY UPDATED_AT DESC
            ) WHERE ROWNUM <= 1";

        return await conn.QueryFirstOrDefaultAsync<PatientSummaryRecord>(sql, new { PatientId = patientId });
    }

    public async Task<PatientSummaryRecord> CreatePatientSummaryAsync(string patientId, string physicianId, string summaryText, string createdBy)
    {
        using var conn = _connectionFactory.CreateConnection();
        var summaryId = Guid.NewGuid().ToString();

        const string sql = @"
            INSERT INTO PATIENT_SUMMARIES (ID, PATIENT_ID, PHYSICIAN_ID, SUMMARY_TEXT, CREATED_AT, UPDATED_AT, LAST_EDITED_BY)
            VALUES (:summaryId, :patientId, :physicianId, :summaryText, SYSTIMESTAMP, SYSTIMESTAMP, :createdBy)";

        await conn.ExecuteAsync(sql, new { summaryId, patientId, physicianId, summaryText, createdBy });

        return new PatientSummaryRecord(
            summaryId,
            patientId,
            physicianId,
            summaryText,
            DateTime.UtcNow, // Approximate, or fetch from DB
            DateTime.UtcNow,
            createdBy
        );
    }

    public async Task<bool> UpdatePatientSummaryAsync(string summaryId, string summaryText, string lastEditedBy)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE PATIENT_SUMMARIES
            SET SUMMARY_TEXT = :summaryText,
                UPDATED_AT = SYSTIMESTAMP,
                LAST_EDITED_BY = :lastEditedBy
            WHERE ID = :summaryId";

        var rows = await conn.ExecuteAsync(sql, new { summaryId, summaryText, lastEditedBy });
        return rows > 0;
    }

  public async Task<PatientDetailRecord?> GetPatientByIdAsync(string patientId)
  {
    using var conn = _connectionFactory.CreateConnection();

    const string sql = @"
      SELECT 
        p.ID, 
        p.NAME, 
        p.MRN, 
        TO_CHAR(p.DATE_OF_BIRTH, 'YYYY-MM-DD') as Dob,
        p.GENDER, 
        TO_CHAR(p.ADMISSION_DATE, 'YYYY-MM-DD HH24:MI:SS') as AdmissionDate,
        u.NAME as CurrentUnit,
        p.ROOM_NUMBER as RoomNumber,
        p.DIAGNOSIS,
        p.ALLERGIES,
        p.MEDICATIONS,
        p.NOTES
      FROM PATIENTS p
      LEFT JOIN UNITS u ON p.UNIT_ID = u.ID
      WHERE p.ID = :PatientId";

    // Use a temporary DTO to handle comma-separated lists if stored as strings, 
    // or adjust if they are stored differently. Assuming string for now based on SQL schema.
    // The schema showed ALLERGIES VARCHAR2(1000), MEDICATIONS VARCHAR2(1000).
    
    var result = await conn.QuerySingleOrDefaultAsync<dynamic>(sql, new { PatientId = patientId });

    if (result == null)
        return null;

    var allergies = ((string?)result.ALLERGIES)?.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList() ?? new List<string>();
    var medications = ((string?)result.MEDICATIONS)?.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList() ?? new List<string>();

    return new PatientDetailRecord(
        (string)result.ID,
        (string)result.NAME,
        (string?)result.MRN ?? "",
        (string?)result.DOB ?? "",
        (string?)result.GENDER ?? "",
        (string?)result.ADMISSIONDATE ?? "",
        (string?)result.CURRENTUNIT ?? "",
        (string?)result.ROOMNUMBER ?? "",
        (string?)result.DIAGNOSIS ?? "",
        allergies,
        medications,
        (string?)result.NOTES ?? ""
    );
  }
}
