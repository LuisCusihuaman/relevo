using Dapper;
using Relevo.Core.Interfaces;

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
}

