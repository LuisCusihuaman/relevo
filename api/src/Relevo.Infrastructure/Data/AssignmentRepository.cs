using Dapper;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;

namespace Relevo.Infrastructure.Data;

public class AssignmentRepository(DapperConnectionFactory _connectionFactory) : IAssignmentRepository
{
    public async Task<IReadOnlyList<string>> AssignPatientsAsync(string userId, string shiftId, IEnumerable<string> patientIds)
    {
        using var conn = _connectionFactory.CreateConnection();

        // Remove existing assignments for this user
        await conn.ExecuteAsync("DELETE FROM USER_ASSIGNMENTS WHERE USER_ID = :userId", new { userId });

        // Insert new assignments
        foreach (var patientId in patientIds)
        {
            var assignmentId = $"assign-{Guid.NewGuid().ToString()[..8]}";
            await conn.ExecuteAsync(@"
                INSERT INTO USER_ASSIGNMENTS (ASSIGNMENT_ID, USER_ID, SHIFT_ID, PATIENT_ID, ASSIGNED_AT) 
                VALUES (:assignmentId, :userId, :shiftId, :patientId, SYSTIMESTAMP)",
                new { assignmentId, userId, shiftId, patientId });
        }

        return patientIds.ToList();
    }

    public async Task<(IReadOnlyList<PatientRecord> Patients, int TotalCount)> GetMyPatientsAsync(string userId, int page, int pageSize)
    {
        using var conn = _connectionFactory.CreateConnection();

        // Get total count
        const string countSql = @"
            SELECT COUNT(DISTINCT p.ID) 
            FROM PATIENTS p 
            INNER JOIN USER_ASSIGNMENTS ua ON p.ID = ua.PATIENT_ID 
            WHERE ua.USER_ID = :userId";

        var total = await conn.ExecuteScalarAsync<int>(countSql, new { userId });

        if (total == 0)
            return (Array.Empty<PatientRecord>(), 0);

        var p_ = Math.Max(page, 1);
        var ps = Math.Max(pageSize, 1);
        var offset = (p_ - 1) * ps;

        // Get patients with pagination - using simpler query without complex subquery
        const string pageSql = @"
            SELECT * FROM (
                SELECT 
                    p.ID AS Id, 
                    p.NAME AS Name, 
                    'not-started' AS HandoverStatus,
                    CAST(NULL AS VARCHAR2(255)) AS HandoverId,
                    FLOOR(MONTHS_BETWEEN(SYSDATE, p.DATE_OF_BIRTH) / 12) AS Age,
                    p.ROOM_NUMBER AS Room,
                    p.DIAGNOSIS AS Diagnosis,
                    CAST(NULL AS VARCHAR2(50)) AS Status,
                    CAST(NULL AS VARCHAR2(50)) AS Severity,
                    ROW_NUMBER() OVER (ORDER BY p.NAME) AS rn
                FROM PATIENTS p
                INNER JOIN USER_ASSIGNMENTS ua ON p.ID = ua.PATIENT_ID
                WHERE ua.USER_ID = :userId
            ) WHERE rn > :offset AND rn <= :maxRow";

        var items = await conn.QueryAsync<PatientRecord>(pageSql, new { userId, offset, maxRow = offset + ps });
        return (items.ToList(), total);
    }
}

