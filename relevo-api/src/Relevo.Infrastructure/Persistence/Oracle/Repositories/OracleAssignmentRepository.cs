using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using Relevo.Core.Interfaces;
using Relevo.Infrastructure.Data.Oracle;
using System.Collections.Generic;

namespace Relevo.Infrastructure.Persistence.Oracle.Repositories;

public class OracleAssignmentRepository : IAssignmentRepository
{
    private readonly IOracleConnectionFactory _factory;
    private readonly ILogger<OracleAssignmentRepository> _logger;

    public OracleAssignmentRepository(IOracleConnectionFactory factory, ILogger<OracleAssignmentRepository> logger)
    {
        _factory = factory;
        _logger = logger;
    }

    public AssignmentRecord? GetAssignment(string userId, string shiftId, string patientId)
    {
        using IDbConnection conn = _factory.CreateConnection();
        const string sql = "SELECT ASSIGNMENT_ID AS Id, USER_ID AS UserId, SHIFT_ID AS ShiftId, PATIENT_ID AS PatientId FROM USER_ASSIGNMENTS WHERE USER_ID = :userId AND SHIFT_ID = :shiftId AND PATIENT_ID = :patientId";
        return conn.QueryFirstOrDefault<AssignmentRecord>(sql, new { userId, shiftId, patientId });
    }

    public void CreateAssignment(string assignmentId, string userId, string shiftId, string patientId)
    {
        using IDbConnection conn = _factory.CreateConnection();
        const string sql = "INSERT INTO USER_ASSIGNMENTS (ASSIGNMENT_ID, USER_ID, SHIFT_ID, PATIENT_ID, ASSIGNED_AT) VALUES (:assignmentId, :userId, :shiftId, :patientId, SYSTIMESTAMP)";
        conn.Execute(sql, new { assignmentId, userId, shiftId, patientId });
    }

    // Implement other methods from IAssignmentRepository (e.g., AssignAsync, GetMyPatients) as needed
    public async Task<IReadOnlyList<string>> AssignAsync(string userId, string shiftId, IEnumerable<string> patientIds)
    {
        // Implementation for AssignAsync using Dapper
        using IDbConnection conn = _factory.CreateConnection();
        // Remove existing
        await conn.ExecuteAsync("DELETE FROM USER_ASSIGNMENTS WHERE USER_ID = :userId", new { userId });
        // Insert new
        foreach (var patientId in patientIds)
        {
            var assignmentId = Guid.NewGuid().ToString("N")[..8];
            await conn.ExecuteAsync("INSERT INTO USER_ASSIGNMENTS (ASSIGNMENT_ID, USER_ID, SHIFT_ID, PATIENT_ID, ASSIGNED_AT) VALUES (:assignmentId, :userId, :shiftId, :patientId, SYSTIMESTAMP)", new { assignmentId, userId, shiftId, patientId });
        }
        return patientIds.ToList();
    }

    public (IReadOnlyList<PatientRecord> Patients, int TotalCount) GetMyPatients(string userId, int page, int pageSize)
    {
        // Implementation using Dapper, similar to other repos
        using IDbConnection conn = _factory.CreateConnection();
        // ... (full implementation as in OraclePatientRepository's GetAllPatients but joined with USER_ASSIGNMENTS)
        const string countSql = "SELECT COUNT(DISTINCT p.ID) FROM PATIENTS p INNER JOIN USER_ASSIGNMENTS ua ON p.ID = ua.PATIENT_ID WHERE ua.USER_ID = :userId";
        int total = conn.ExecuteScalar<int>(countSql, new { userId });

        int offset = (page - 1) * pageSize;
        int maxRow = page * pageSize;
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
              INNER JOIN USER_ASSIGNMENTS ua ON p.ID = ua.PATIENT_ID
              LEFT JOIN (
                SELECT ID AS HANDOVER_ID, PATIENT_ID, STATUS, COMPLETED_AT, CANCELLED_AT, REJECTED_AT, EXPIRED_AT, ACCEPTED_AT, STARTED_AT, READY_AT,
                       ROW_NUMBER() OVER (PARTITION BY PATIENT_ID ORDER BY CREATED_AT DESC) AS rn
                FROM HANDOVERS
              ) h ON p.ID = h.PATIENT_ID AND h.rn = 1
              LEFT JOIN HANDOVER_PATIENT_DATA hpd ON h.HANDOVER_ID = hpd.HANDOVER_ID
              WHERE ua.USER_ID = :userId
              ORDER BY p.ID
        ) WHERE rn > :offset AND rn <= :maxRow";
        var items = conn.Query<PatientRecord>(pageSql, new { userId, offset, maxRow }).ToList();
        return (items, total);
    }
}
