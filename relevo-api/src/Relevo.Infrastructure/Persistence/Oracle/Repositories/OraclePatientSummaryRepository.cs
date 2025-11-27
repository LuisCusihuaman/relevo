using System;
using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using Relevo.Core.Interfaces;
using Relevo.Infrastructure.Data.Oracle;

namespace Relevo.Infrastructure.Persistence.Oracle.Repositories;

public class OraclePatientSummaryRepository : IPatientSummaryRepository
{
    private readonly IOracleConnectionFactory _factory;
    private readonly ILogger<OraclePatientSummaryRepository> _logger;

    public OraclePatientSummaryRepository(IOracleConnectionFactory factory, ILogger<OraclePatientSummaryRepository> logger)
    {
        _factory = factory;
        _logger = logger;
    }

    public PatientSummaryRecord? GetPatientSummary(string patientId)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();
            const string sql = @"
                SELECT * FROM (
                    SELECT ID AS Id,
                           PATIENT_ID AS PatientId,
                           PHYSICIAN_ID AS PhysicianId,
                           SUMMARY_TEXT AS SummaryText,
                           CREATED_AT AS CreatedAt,
                           UPDATED_AT AS UpdatedAt,
                           LAST_EDITED_BY AS LastEditedBy
                    FROM PATIENT_SUMMARIES
                    WHERE PATIENT_ID = :patientId
                    ORDER BY UPDATED_AT DESC
                ) WHERE ROWNUM = 1";

            return conn.QueryFirstOrDefault<PatientSummaryRecord>(sql, new { patientId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get patient summary for patient {PatientId}", patientId);
            throw;
        }
    }

    public PatientSummaryRecord CreatePatientSummary(string patientId, string physicianId, string summaryText, string createdBy)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();
            var summaryId = Guid.NewGuid().ToString();

            const string sql = @"
                INSERT INTO PATIENT_SUMMARIES (ID, PATIENT_ID, PHYSICIAN_ID, SUMMARY_TEXT, CREATED_AT, UPDATED_AT, LAST_EDITED_BY)
                VALUES (:summaryId, :patientId, :physicianId, :summaryText, SYSTIMESTAMP, SYSTIMESTAMP, :createdBy)";

            conn.Execute(sql, new { summaryId, patientId, physicianId, summaryText, createdBy });

            return new PatientSummaryRecord(
                summaryId,
                patientId,
                physicianId,
                summaryText,
                DateTime.UtcNow,
                DateTime.UtcNow,
                createdBy
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create patient summary for patient {PatientId}", patientId);
            throw;
        }
    }

    public bool UpdatePatientSummary(string summaryId, string summaryText, string lastEditedBy)
    {
        try
        {
            using IDbConnection conn = _factory.CreateConnection();
            const string sql = @"
                UPDATE PATIENT_SUMMARIES
                SET SUMMARY_TEXT = :summaryText,
                    UPDATED_AT = SYSTIMESTAMP,
                    LAST_EDITED_BY = :lastEditedBy
                WHERE ID = :summaryId";

            var result = conn.Execute(sql, new { summaryId, summaryText, lastEditedBy });
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update patient summary {SummaryId}", summaryId);
            throw;
        }
    }
}
