using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using Relevo.Core.Interfaces;
using Relevo.Infrastructure.Data.Oracle;
using System.Collections.Generic;
using System.Linq;

namespace Relevo.Infrastructure.Persistence.Oracle.Repositories;

public class OraclePatientRepository : IPatientRepository
{
    private readonly IOracleConnectionFactory _factory;
    private readonly ILogger<OraclePatientRepository> _logger;

    public OraclePatientRepository(IOracleConnectionFactory factory, ILogger<OraclePatientRepository> logger)
    {
        _factory = factory;
        _logger = logger;
    }

    public (IReadOnlyList<PatientRecord> Patients, int TotalCount) GetPatientsByUnit(string unitId, int page, int pageSize)
    {
        using IDbConnection conn = _factory.CreateConnection();
        int p = Math.Max(page, 1);
        int ps = Math.Max(pageSize, 1);

        // Count all patients in unit (assignment status tracked via handover state)
        const string countSql = @"
            SELECT COUNT(1) FROM PATIENTS p
            WHERE p.UNIT_ID = :unitId";

        // Get paged patients using ROWNUM for Oracle 11g compatibility
        const string pageSql = @"SELECT Id, Name, HandoverStatus, HandoverId, Age, Room, Diagnosis, Status, Severity FROM (
          SELECT p.ID AS Id, p.NAME AS Name, 'NotStarted' AS HandoverStatus, CAST(NULL AS VARCHAR(255)) AS HandoverId,
            FLOOR((SYSDATE - p.DATE_OF_BIRTH)/365.25) AS Age, p.ROOM_NUMBER AS Room, p.DIAGNOSIS AS Diagnosis,
            CASE
              WHEN h.STATUS = 'Completed' AND h.COMPLETED_AT IS NOT NULL THEN 'Completed'
              WHEN h.CANCELLED_AT IS NOT NULL THEN 'Cancelled'
              WHEN h.REJECTED_AT IS NOT NULL THEN 'Rejected'
              WHEN h.EXPIRED_AT IS NOT NULL THEN 'Expired'
              WHEN h.ACCEPTED_AT IS NOT NULL THEN 'Accepted'
              WHEN h.STARTED_AT IS NOT NULL THEN 'InProgress'
              WHEN h.READY_AT IS NOT NULL THEN 'Ready'
              ELSE 'Draft'
            END AS Status,
            hpd.ILLNESS_SEVERITY AS Severity,
            ROWNUM as rn
            FROM PATIENTS p
            LEFT JOIN (
              SELECT ID, PATIENT_ID, STATUS, COMPLETED_AT, CANCELLED_AT, REJECTED_AT, EXPIRED_AT, ACCEPTED_AT, STARTED_AT, READY_AT,
                     ROW_NUMBER() OVER (PARTITION BY PATIENT_ID ORDER BY CREATED_AT DESC) AS rn
              FROM HANDOVERS
            ) h ON p.ID = h.PATIENT_ID AND h.rn = 1
            LEFT JOIN HANDOVER_PATIENT_DATA hpd ON h.ID = hpd.HANDOVER_ID
            WHERE p.UNIT_ID = :unitId
            ORDER BY p.ID
        ) WHERE rn > :offset AND rn <= :maxRow";

        int total = conn.ExecuteScalar<int>(countSql, new { unitId });
        int offset = ((p - 1) * ps);
        int maxRow = (p * ps);
        var items = conn.Query<PatientRecord>(pageSql, new { unitId, offset, maxRow }).ToList();
        return (items, total);
    }

    public (IReadOnlyList<PatientRecord> Patients, int TotalCount) GetAllPatients(int page, int pageSize)
    {
        using IDbConnection conn = _factory.CreateConnection();
        int p = Math.Max(page, 1);
        int ps = Math.Max(pageSize, 1);
        const string countSql = "SELECT COUNT(1) FROM PATIENTS";
        const string pageSql = @"SELECT Id, Name, HandoverStatus, HandoverId, Age, Room, Diagnosis, Status, Severity FROM (
            SELECT p.ID AS Id, p.NAME AS Name, 'NotStarted' AS HandoverStatus, CAST(NULL AS VARCHAR(255)) AS HandoverId,
                CAST(FLOOR((SYSDATE - p.DATE_OF_BIRTH)/365.25) AS NUMBER) AS Age, p.ROOM_NUMBER AS Room, p.DIAGNOSIS AS Diagnosis,
                CASE
                  WHEN h.STATUS = 'Completed' AND h.COMPLETED_AT IS NOT NULL THEN 'Completed'
                  WHEN h.CANCELLED_AT IS NOT NULL THEN 'Cancelled'
                  WHEN h.REJECTED_AT IS NOT NULL THEN 'Rejected'
                  WHEN h.EXPIRED_AT IS NOT NULL THEN 'Expired'
                  WHEN h.ACCEPTED_AT IS NOT NULL THEN 'Accepted'
                  WHEN h.STARTED_AT IS NOT NULL THEN 'InProgress'
                  WHEN h.READY_AT IS NOT NULL THEN 'Ready'
                  ELSE 'Draft'
                END AS Status,
                hpd.ILLNESS_SEVERITY AS Severity,
                ROWNUM as rn
              FROM PATIENTS p
              LEFT JOIN (
                SELECT ID, PATIENT_ID, STATUS, COMPLETED_AT, CANCELLED_AT, REJECTED_AT, EXPIRED_AT, ACCEPTED_AT, STARTED_AT, READY_AT,
                       ROW_NUMBER() OVER (PARTITION BY PATIENT_ID ORDER BY CREATED_AT DESC) AS rn
                FROM HANDOVERS
              ) h ON p.ID = h.PATIENT_ID AND h.rn = 1
              LEFT JOIN HANDOVER_PATIENT_DATA hpd ON h.ID = hpd.HANDOVER_ID
              ORDER BY p.ID
        ) WHERE rn > :offset AND rn <= :maxRow";

        int total = conn.ExecuteScalar<int>(countSql);
        int offset = ((p - 1) * ps);
        int maxRow = (p * ps);
        var items = conn.Query<PatientRecord>(pageSql, new { offset, maxRow }).ToList();
        return (items, total);
    }

    public PatientDetailRecord? GetPatientById(string patientId)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();

            const string patientSql = @"
              SELECT p.ID, p.NAME, p.DATE_OF_BIRTH, p.GENDER, p.ADMISSION_DATE,
                     p.UNIT_ID, p.ROOM_NUMBER, p.DIAGNOSIS, p.ALLERGIES, p.MEDICATIONS, p.NOTES, p.MRN
              FROM PATIENTS p
              WHERE p.ID = :patientId";

            var row = conn.QueryFirstOrDefault(patientSql, new { patientId });

            if (row == null)
            {
                return null;
            }

            // Parse allergies and medications (assuming they are stored as comma-separated strings)
            var allergies = ParseCommaSeparatedString(row.ALLERGIES);
            var medications = ParseCommaSeparatedString(row.MEDICATIONS);

            return new PatientDetailRecord(
                Id: row.ID,
                Name: row.NAME ?? "Unknown",
                Mrn: row.MRN ?? GenerateMrn(row.ID), // Use MRN from database, fallback to generated
                Dob: row.DATE_OF_BIRTH?.ToString("yyyy-MM-dd") ?? "",
                Gender: row.GENDER ?? "Unknown",
                AdmissionDate: row.ADMISSION_DATE?.ToString("yyyy-MM-dd HH:mm:ss") ?? "",
                CurrentUnit: row.UNIT_ID ?? "",
                RoomNumber: row.ROOM_NUMBER ?? "",
                Diagnosis: row.DIAGNOSIS ?? "",
                Allergies: allergies,
                Medications: medications,
                Notes: row.NOTES ?? ""
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get patient {PatientId}", patientId);
            throw;
        }
    }

    private string GenerateMrn(string patientId)
    {
        // Generate a simple MRN based on patient ID
        // For example: pat-001 becomes MRN001
        var numberPart = patientId.Split('-').LastOrDefault() ?? "000";
        return $"MRN{numberPart.PadLeft(3, '0')}";
    }

    private List<string> ParseCommaSeparatedString(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return new List<string>();
        }

        return value.Split(',')
            .Select(item => item.Trim())
            .Where(item => !string.IsNullOrEmpty(item))
            .ToList();
    }
}
