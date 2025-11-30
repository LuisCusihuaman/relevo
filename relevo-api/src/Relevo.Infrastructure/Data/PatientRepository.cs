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

    public Task<PatientSummaryRecord?> GetPatientSummaryAsync(string patientId)
    {
        // PATIENT_SUMMARIES table removed - patient summary now comes from latest handover
        // This method should be called with handoverId, not patientId
        // Keeping for backward compatibility but will return null
        // Use GetPatientSummaryFromHandoverAsync instead
        return Task.FromResult<PatientSummaryRecord?>(null);
    }

    public async Task<PatientSummaryRecord?> GetPatientSummaryFromHandoverAsync(string handoverId)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT 
                hc.HANDOVER_ID as Id,
                h.PATIENT_ID as PatientId,
                h.FROM_USER_ID as PhysicianId,
                NVL(hc.PATIENT_SUMMARY, '') as SummaryText,
                h.CREATED_AT as CreatedAt,
                hc.UPDATED_AT as UpdatedAt,
                NVL(hc.LAST_EDITED_BY, h.FROM_USER_ID) as LastEditedBy
            FROM HANDOVER_CONTENTS hc
            JOIN HANDOVERS h ON hc.HANDOVER_ID = h.ID
            WHERE hc.HANDOVER_ID = :HandoverId";

        return await conn.QueryFirstOrDefaultAsync<PatientSummaryRecord>(sql, new { HandoverId = handoverId });
    }

    public async Task<PatientSummaryRecord> CreatePatientSummaryAsync(string handoverId, string summaryText, string createdBy)
    {
        // PATIENT_SUMMARIES removed - now updates HANDOVER_CONTENTS.PATIENT_SUMMARY
        using var conn = _connectionFactory.CreateConnection();

        // Update or insert HANDOVER_CONTENTS
        await conn.ExecuteAsync(@"
            MERGE INTO HANDOVER_CONTENTS hc
            USING (SELECT :handoverId AS HANDOVER_ID FROM dual) src ON (hc.HANDOVER_ID = src.HANDOVER_ID)
            WHEN MATCHED THEN
                UPDATE SET PATIENT_SUMMARY = :summaryText, PATIENT_SUMMARY_STATUS = 'Draft', 
                           LAST_EDITED_BY = :createdBy, UPDATED_AT = LOCALTIMESTAMP
            WHEN NOT MATCHED THEN
                INSERT (HANDOVER_ID, PATIENT_SUMMARY, PATIENT_SUMMARY_STATUS, LAST_EDITED_BY, UPDATED_AT,
                        ILLNESS_SEVERITY, SITUATION_AWARENESS, SYNTHESIS, SA_STATUS, SYNTHESIS_STATUS)
                VALUES (:handoverId, :summaryText, 'Draft', :createdBy, LOCALTIMESTAMP,
                        'Stable', NULL, NULL, 'Draft', 'Draft')",
            new { handoverId, summaryText, createdBy });

        // Fetch created/updated record
        var handover = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT PATIENT_ID, FROM_USER_ID, CREATED_AT FROM HANDOVERS WHERE ID = :handoverId",
            new { handoverId });

        if (handover == null)
        {
            throw new InvalidOperationException($"Handover {handoverId} not found after creation");
        }

        return new PatientSummaryRecord(
            handoverId,
            (string)handover.PATIENT_ID,
            (string)handover.FROM_USER_ID,
            summaryText,
            ((DateTime)handover.CREATED_AT).ToUniversalTime(),
            DateTime.UtcNow,
            createdBy
        );
    }

    public async Task<bool> UpdatePatientSummaryAsync(string handoverId, string summaryText, string lastEditedBy)
    {
        // PATIENT_SUMMARIES removed - now updates HANDOVER_CONTENTS.PATIENT_SUMMARY
        // Use MERGE to handle cases where HANDOVER_CONTENTS row doesn't exist yet
        using var conn = _connectionFactory.CreateConnection();
        const string sql = @"
            MERGE INTO HANDOVER_CONTENTS hc
            USING (SELECT :handoverId AS HANDOVER_ID FROM dual) src ON (hc.HANDOVER_ID = src.HANDOVER_ID)
            WHEN MATCHED THEN
                UPDATE SET PATIENT_SUMMARY = :summaryText, PATIENT_SUMMARY_STATUS = 'Draft',
                           LAST_EDITED_BY = :lastEditedBy, UPDATED_AT = LOCALTIMESTAMP
            WHEN NOT MATCHED THEN
                INSERT (HANDOVER_ID, PATIENT_SUMMARY, PATIENT_SUMMARY_STATUS, LAST_EDITED_BY, UPDATED_AT,
                        ILLNESS_SEVERITY, SITUATION_AWARENESS, SYNTHESIS, SA_STATUS, SYNTHESIS_STATUS)
                VALUES (:handoverId, :summaryText, 'Draft', :lastEditedBy, LOCALTIMESTAMP,
                        'Stable', NULL, NULL, 'Draft', 'Draft')";

        var rows = await conn.ExecuteAsync(sql, new { handoverId, summaryText, lastEditedBy });
        return rows > 0;
    }

    public async Task<IReadOnlyList<PatientActionItemRecord>> GetPatientActionItemsAsync(string patientId)
    {
        using var conn = _connectionFactory.CreateConnection();
        // Query to join action items with handovers to filter by patient
        // Updated schema: HANDOVER_ACTION_ITEMS (HANDOVER_ID) -> HANDOVERS (ID, PATIENT_ID, FROM_USER_ID, FROM_SHIFT_ID)
        const string sql = @"
            SELECT 
                ai.ID,
                ai.HANDOVER_ID as HandoverId,
                ai.DESCRIPTION,
                ai.IS_COMPLETED as IsCompleted,
                ai.CREATED_AT as CreatedAt,
                h.FROM_USER_ID as CreatedBy, -- Column renamed
                s.NAME as ShiftName -- Join SHIFTS table
            FROM HANDOVER_ACTION_ITEMS ai
            JOIN HANDOVERS h ON ai.HANDOVER_ID = h.ID
            LEFT JOIN SHIFTS s ON h.FROM_SHIFT_ID = s.ID -- For ShiftName
            WHERE h.PATIENT_ID = :PatientId
            ORDER BY ai.CREATED_AT DESC";

        var items = await conn.QueryAsync<dynamic>(sql, new { PatientId = patientId });

        return items.Select(i => new PatientActionItemRecord(
            (string)i.ID,
            (string)i.HANDOVERID,
            (string)i.DESCRIPTION,
            ((int)i.ISCOMPLETED) == 1,
            (DateTime)i.CREATEDAT,
            (string)i.CREATEDBY,
            (string)i.SHIFTNAME
        )).ToList();
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
