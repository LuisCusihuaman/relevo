using System.Data;
using Dapper;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;

namespace Relevo.Infrastructure.Data;

public class PatientRepository(DapperConnectionFactory _connectionFactory) : IPatientRepository
{
  public async Task<(IReadOnlyList<PatientRecord> Patients, int TotalCount)> GetPatientsByUnitAsync(string unitId, int page, int pageSize, string? userId = null)
  {
    using var conn = _connectionFactory.CreateConnection();

    var p = Math.Max(page, 1);
    var ps = Math.Max(pageSize, 1);
    var offset = (p - 1) * ps;

    // Count - always count all patients in the unit
    var total = await conn.ExecuteScalarAsync<int>(
        "SELECT COUNT(*) FROM PATIENTS WHERE UNIT_ID = :UnitId",
        new { UnitId = unitId });

    if (total == 0)
        return (Array.Empty<PatientRecord>(), 0);

    // Query - set Status to 'assigned' if patient is already assigned to the user, otherwise 'pending'
    string sql;
    object queryParams;
    
    if (!string.IsNullOrEmpty(userId))
    {
        sql = @"
          SELECT ID, NAME, HandoverStatus, HandoverId, Age, Room, DIAGNOSIS, Status, Severity, AssignedToName FROM (
            SELECT
                p.ID,
                p.NAME,
                COALESCE(
                    (SELECT h.CURRENT_STATE FROM HANDOVERS h 
                     WHERE h.PATIENT_ID = p.ID 
                       AND h.CURRENT_STATE NOT IN ('Completed', 'Cancelled')
                       AND ROWNUM = 1),
                    'not-started'
                ) as HandoverStatus,
                (SELECT h.ID FROM HANDOVERS h 
                 WHERE h.PATIENT_ID = p.ID 
                   AND h.CURRENT_STATE NOT IN ('Completed', 'Cancelled')
                   AND ROWNUM = 1) as HandoverId,
                ROUND(MONTHS_BETWEEN(SYSDATE, p.DATE_OF_BIRTH)/12, 1) as Age,
                p.ROOM_NUMBER as Room,
                p.DIAGNOSIS,
                CASE 
                    WHEN EXISTS (
                        SELECT 1 
                        FROM SHIFT_COVERAGE sc
                        INNER JOIN SHIFT_INSTANCES si ON sc.SHIFT_INSTANCE_ID = si.ID
                        WHERE sc.PATIENT_ID = p.ID
                          AND (si.END_AT >= SYSDATE OR si.START_AT >= SYSDATE - INTERVAL '24' HOUR)
                    ) THEN 'assigned'
                    ELSE 'pending'
                END as Status,
                COALESCE(
                    (SELECT hc.ILLNESS_SEVERITY 
                     FROM HANDOVERS h
                     INNER JOIN HANDOVER_CONTENTS hc ON h.ID = hc.HANDOVER_ID
                     WHERE h.PATIENT_ID = p.ID 
                       AND h.CURRENT_STATE NOT IN ('Completed', 'Cancelled')
                       AND ROWNUM = 1),
                    'Stable'
                ) as Severity,
                (SELECT u.FULL_NAME 
                 FROM SHIFT_COVERAGE sc
                 INNER JOIN SHIFT_INSTANCES si ON sc.SHIFT_INSTANCE_ID = si.ID
                 INNER JOIN USERS u ON sc.RESPONSIBLE_USER_ID = u.ID
                 WHERE sc.PATIENT_ID = p.ID
                   AND (si.END_AT >= SYSDATE OR si.START_AT >= SYSDATE - INTERVAL '24' HOUR)
                   AND ROWNUM = 1) as AssignedToName,
                ROW_NUMBER() OVER (ORDER BY p.NAME) AS RN
            FROM PATIENTS p
            WHERE p.UNIT_ID = :UnitId
          )
          WHERE RN BETWEEN :StartRow AND :EndRow";
        queryParams = new { UnitId = unitId, UserId = userId, StartRow = offset + 1, EndRow = offset + ps };
    }
    else
    {
        sql = @"
          SELECT ID, NAME, HandoverStatus, HandoverId, Age, Room, DIAGNOSIS, Status, Severity, AssignedToName FROM (
            SELECT
                p.ID,
                p.NAME,
                COALESCE(
                    (SELECT h.CURRENT_STATE FROM HANDOVERS h 
                     WHERE h.PATIENT_ID = p.ID 
                       AND h.CURRENT_STATE NOT IN ('Completed', 'Cancelled')
                       AND ROWNUM = 1),
                    'not-started'
                ) as HandoverStatus,
                (SELECT h.ID FROM HANDOVERS h 
                 WHERE h.PATIENT_ID = p.ID 
                   AND h.CURRENT_STATE NOT IN ('Completed', 'Cancelled')
                   AND ROWNUM = 1) as HandoverId,
                ROUND(MONTHS_BETWEEN(SYSDATE, p.DATE_OF_BIRTH)/12, 1) as Age,
                p.ROOM_NUMBER as Room,
                p.DIAGNOSIS,
                CASE 
                    WHEN EXISTS (
                        SELECT 1 
                        FROM SHIFT_COVERAGE sc
                        INNER JOIN SHIFT_INSTANCES si ON sc.SHIFT_INSTANCE_ID = si.ID
                        WHERE sc.PATIENT_ID = p.ID
                          AND (si.END_AT >= SYSDATE OR si.START_AT >= SYSDATE - INTERVAL '24' HOUR)
                    ) THEN 'assigned'
                    ELSE 'pending'
                END as Status,
                COALESCE(
                    (SELECT hc.ILLNESS_SEVERITY 
                     FROM HANDOVERS h
                     INNER JOIN HANDOVER_CONTENTS hc ON h.ID = hc.HANDOVER_ID
                     WHERE h.PATIENT_ID = p.ID 
                       AND h.CURRENT_STATE NOT IN ('Completed', 'Cancelled')
                       AND ROWNUM = 1),
                    'Stable'
                ) as Severity,
                (SELECT u.FULL_NAME 
                 FROM SHIFT_COVERAGE sc
                 INNER JOIN SHIFT_INSTANCES si ON sc.SHIFT_INSTANCE_ID = si.ID
                 INNER JOIN USERS u ON sc.RESPONSIBLE_USER_ID = u.ID
                 WHERE sc.PATIENT_ID = p.ID
                   AND (si.END_AT >= SYSDATE OR si.START_AT >= SYSDATE - INTERVAL '24' HOUR)
                   AND ROWNUM = 1) as AssignedToName,
                ROW_NUMBER() OVER (ORDER BY p.NAME) AS RN
            FROM PATIENTS p
            WHERE p.UNIT_ID = :UnitId
          )
          WHERE RN BETWEEN :StartRow AND :EndRow";
        queryParams = new { UnitId = unitId, StartRow = offset + 1, EndRow = offset + ps };
    }

    var patients = await conn.QueryAsync<PatientRecord>(sql, queryParams);

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
        SELECT ID, NAME, HandoverStatus, HandoverId, Age, Room, DIAGNOSIS, Status, Severity, Unit, AssignedToName FROM (
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
              u.NAME as Unit,
              (SELECT u2.FULL_NAME 
               FROM SHIFT_COVERAGE sc
               INNER JOIN SHIFT_INSTANCES si ON sc.SHIFT_INSTANCE_ID = si.ID
               INNER JOIN USERS u2 ON sc.RESPONSIBLE_USER_ID = u2.ID
               WHERE sc.PATIENT_ID = p.ID
                 AND (si.END_AT >= SYSDATE OR si.START_AT >= SYSDATE - INTERVAL '24' HOUR)
                 AND ROWNUM = 1) as AssignedToName,
              ROW_NUMBER() OVER (ORDER BY p.NAME) AS RN
          FROM PATIENTS p
          LEFT JOIN UNITS u ON p.UNIT_ID = u.ID
        )
        WHERE RN BETWEEN :StartRow AND :EndRow";

      var patients = await conn.QueryAsync<PatientRecord>(sql, new { StartRow = offset + 1, EndRow = offset + ps });

      return (patients.ToList(), total);
    }


    public async Task<PatientSummaryRecord?> GetPatientSummaryFromHandoverAsync(string handoverId)
    {
        using var conn = _connectionFactory.CreateConnection();
        // V3 Schema: Uses SENDER_USER_ID, CREATED_BY_USER_ID
        const string sql = @"
            SELECT 
                hc.HANDOVER_ID as Id,
                h.PATIENT_ID as PatientId,
                COALESCE(h.SENDER_USER_ID, h.CREATED_BY_USER_ID) as PhysicianId, -- V3: SENDER_USER_ID or CREATED_BY_USER_ID
                NVL(hc.PATIENT_SUMMARY, '') as SummaryText,
                h.CREATED_AT as CreatedAt,
                hc.UPDATED_AT as UpdatedAt,
                NVL(hc.LAST_EDITED_BY, COALESCE(h.SENDER_USER_ID, h.CREATED_BY_USER_ID)) as LastEditedBy -- V3: SENDER_USER_ID or CREATED_BY_USER_ID
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

        // Fetch created/updated record - V3 Schema: Uses CREATED_BY_USER_ID or SENDER_USER_ID
        var handover = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT PATIENT_ID, CREATED_BY_USER_ID, SENDER_USER_ID, CREATED_AT FROM HANDOVERS WHERE ID = :handoverId",
            new { handoverId });

        if (handover == null)
        {
            throw new InvalidOperationException($"Handover {handoverId} not found after creation");
        }

        // V3: Use SENDER_USER_ID if available, otherwise CREATED_BY_USER_ID
        var physicianId = (string?)handover.SENDER_USER_ID ?? (string?)handover.CREATED_BY_USER_ID ?? createdBy;

        return new PatientSummaryRecord(
            handoverId,
            (string)handover.PATIENT_ID,
            physicianId,
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
        // V3 Schema: Uses SHIFT_WINDOW_ID, SENDER_USER_ID, CREATED_BY_USER_ID
        // Get shift name from SHIFT_WINDOWS -> SHIFT_INSTANCES -> SHIFTS
        const string sql = @"
            SELECT 
                ai.ID,
                ai.HANDOVER_ID as HandoverId,
                ai.DESCRIPTION,
                ai.IS_COMPLETED as IsCompleted,
                ai.CREATED_AT as CreatedAt,
                COALESCE(h.SENDER_USER_ID, h.CREATED_BY_USER_ID) as CreatedBy, -- V3: SENDER_USER_ID or CREATED_BY_USER_ID
                s_from.NAME as ShiftName -- V3: From shift name via SHIFT_WINDOWS
            FROM HANDOVER_ACTION_ITEMS ai
            JOIN HANDOVERS h ON ai.HANDOVER_ID = h.ID
            LEFT JOIN SHIFT_WINDOWS sw ON h.SHIFT_WINDOW_ID = sw.ID -- V3: Join SHIFT_WINDOWS
            LEFT JOIN SHIFT_INSTANCES si_from ON sw.FROM_SHIFT_INSTANCE_ID = si_from.ID -- V3: From shift instance
            LEFT JOIN SHIFTS s_from ON si_from.SHIFT_ID = s_from.ID -- V3: From shift template
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
        p.NOTES,
        p.WEIGHT,
        p.HEIGHT
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
        (string?)result.NOTES ?? "",
        (string?)result.WEIGHT,
        (string?)result.HEIGHT
    );
  }

  public async Task<bool> DeletePatientAsync(string patientId)
  {
    using var conn = _connectionFactory.CreateConnection();
    
    // First, check if patient exists
    var exists = await conn.ExecuteScalarAsync<int>(
        "SELECT COUNT(*) FROM PATIENTS WHERE ID = :PatientId",
        new { PatientId = patientId });

    if (exists == 0)
    {
        return false;
    }

    // Delete related records first (in reverse order of dependencies)
    // 1. Delete shift coverage (assignments)
    await conn.ExecuteAsync(
        "DELETE FROM SHIFT_COVERAGE WHERE PATIENT_ID = :PatientId",
        new { PatientId = patientId });

    // 2. Delete handovers (need to handle self-referencing FK first)
    // First, set PREVIOUS_HANDOVER_ID to NULL for handovers that reference other handovers of the same patient
    await conn.ExecuteAsync(
        "UPDATE HANDOVERS SET PREVIOUS_HANDOVER_ID = NULL WHERE PATIENT_ID = :PatientId AND PREVIOUS_HANDOVER_ID IS NOT NULL",
        new { PatientId = patientId });

    // Then delete all handovers for this patient (cascade will delete handover contents, action items, etc.)
    await conn.ExecuteAsync(
        "DELETE FROM HANDOVERS WHERE PATIENT_ID = :PatientId",
        new { PatientId = patientId });

    // 3. Finally, delete the patient
    var deleted = await conn.ExecuteAsync(
        "DELETE FROM PATIENTS WHERE ID = :PatientId",
        new { PatientId = patientId });

    return deleted > 0;
  }
}
