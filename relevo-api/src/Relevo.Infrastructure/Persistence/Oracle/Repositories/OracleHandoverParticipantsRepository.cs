using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Microsoft.Extensions.Logging;
using Relevo.Core.Interfaces;
using Relevo.Infrastructure.Data.Oracle;

namespace Relevo.Infrastructure.Persistence.Oracle.Repositories;

public class OracleHandoverParticipantsRepository : IHandoverParticipantsRepository
{
    private readonly IOracleConnectionFactory _factory;
    private readonly ILogger<OracleHandoverParticipantsRepository> _logger;

    public OracleHandoverParticipantsRepository(IOracleConnectionFactory factory, ILogger<OracleHandoverParticipantsRepository> logger)
    {
        _factory = factory;
        _logger = logger;
    }

    public IReadOnlyList<HandoverParticipantRecord> GetHandoverParticipants(string handoverId)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();

            const string sql = @"
                SELECT ID, HANDOVER_ID as HandoverId, USER_ID as UserId, USER_NAME as UserName, USER_ROLE as UserRole, STATUS,
                       JOINED_AT as JoinedAt, LAST_ACTIVITY as LastActivity
                FROM HANDOVER_PARTICIPANTS
                WHERE HANDOVER_ID = :handoverId
                ORDER BY JOINED_AT";

            var participants = conn.Query<HandoverParticipantRecord>(sql, new { handoverId }).ToList();

            // If no participants found, return a default list with the assigned user
            if (!participants.Any())
            {
                // Get the handover creator as the default participant
                const string creatorSql = @"
                    SELECT TO_DOCTOR_ID as USER_ID, 'Assigned Physician' as USER_NAME, 'Doctor' as USER_ROLE
                    FROM HANDOVERS
                    WHERE ID = :handoverId";

                var creator = conn.QueryFirstOrDefault(creatorSql, new { handoverId });

                if (creator != null)
                {
                    participants.Add(new HandoverParticipantRecord(
                        Id: $"participant-{handoverId}-default",
                        HandoverId: handoverId,
                        UserId: creator.USER_ID,
                        UserName: creator.USER_NAME ?? "Assigned Physician",
                        UserRole: creator.USER_ROLE,
                        Status: "active",
                        JoinedAt: DateTime.Now,
                        LastActivity: DateTime.Now
                    ));
                }
            }

            return participants;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get participants for handover {HandoverId}", handoverId);
            return Array.Empty<HandoverParticipantRecord>();
        }
    }
}
