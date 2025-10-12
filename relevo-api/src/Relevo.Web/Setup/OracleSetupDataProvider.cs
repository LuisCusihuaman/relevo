using System.Data;
using Dapper;
using Relevo.Infrastructure.Data.Oracle;
using Relevo.Web.Patients;
using Relevo.Web.Me;
using Relevo.Core.Interfaces;

// Use specific types from Core layer to avoid conflicts
using PatientRecord = Relevo.Core.Interfaces.PatientRecord;
using UnitRecord = Relevo.Core.Interfaces.UnitRecord;
using ShiftRecord = Relevo.Core.Interfaces.ShiftRecord;
using HandoverRecord = Relevo.Core.Interfaces.HandoverRecord;

namespace Relevo.Web.Setup;

public class OracleSetupDataProvider(IOracleConnectionFactory _factory) : ISetupDataProvider
{
  // Using Oracle database for persistent assignments

  public IReadOnlyList<UnitRecord> GetUnits()
  {
    using IDbConnection conn = _factory.CreateConnection();
    const string sql = "SELECT ID AS Id, NAME AS Name FROM UNITS ORDER BY ID";
    return conn.Query<UnitRecord>(sql).ToList();
  }

  public IReadOnlyList<ShiftRecord> GetShifts()
  {
    using IDbConnection conn = _factory.CreateConnection();
    const string sql = @"SELECT ID AS Id, NAME AS Name, START_TIME AS StartTime, END_TIME AS EndTime FROM SHIFTS ORDER BY ID";
    return conn.Query<ShiftRecord>(sql).ToList();
  }

  public (IReadOnlyList<PatientRecord> Patients, int TotalCount) GetPatientsByUnit(string unitId, int page, int pageSize)
  {
    using IDbConnection conn = _factory.CreateConnection();
    int p = Math.Max(page, 1);
    int ps = Math.Max(pageSize, 1);
    const string countSql = "SELECT COUNT(1) FROM PATIENTS WHERE UNIT_ID = :unitId";
    const string pageSql = @"SELECT ID AS Id, NAME AS Name FROM (
      SELECT ID, NAME, ROW_NUMBER() OVER (ORDER BY ID) AS RN
      FROM PATIENTS WHERE UNIT_ID = :unitId
    ) WHERE RN BETWEEN :startRow AND :endRow";

    int total = conn.ExecuteScalar<int>(countSql, new { unitId });
    int startRow = ((p - 1) * ps) + 1;
    int endRow = p * ps;
    var items = conn.Query<PatientRecord>(pageSql, new { unitId, startRow, endRow }).ToList();
    return (items, total);
  }

  public void Assign(string userId, string shiftId, IEnumerable<string> patientIds)
  {
    using IDbConnection conn = _factory.CreateConnection();

    // Remove existing assignments for this user
    conn.Execute("DELETE FROM USER_ASSIGNMENTS WHERE USER_ID = :userId",
        new { userId });

    // Insert new assignments
    foreach (var patientId in patientIds)
    {
      conn.Execute(@"
        INSERT INTO USER_ASSIGNMENTS (USER_ID, SHIFT_ID, PATIENT_ID)
        VALUES (:userId, :shiftId, :patientId)",
        new { userId, shiftId, patientId });
    }
  }

  public (IReadOnlyList<PatientRecord> Patients, int TotalCount) GetMyPatients(string userId, int page, int pageSize)
  {
    using IDbConnection conn = _factory.CreateConnection();

    // Get total count of assigned patients
    const string countSql = "SELECT COUNT(*) FROM USER_ASSIGNMENTS WHERE USER_ID = :userId";
    int total = conn.ExecuteScalar<int>(countSql, new { userId });

    if (total == 0)
      return (Array.Empty<PatientRecord>(), 0);

    // Get assigned patients with pagination
    int p = Math.Max(page, 1);
    int ps = Math.Max(pageSize, 1);
    int offset = (p - 1) * ps;

    const string patientsSql = @"
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
      hpd.ILLNESS_SEVERITY AS Severity
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
      OFFSET :offset ROWS FETCH NEXT :pageSize ROWS ONLY";

    var patients = conn.Query<PatientRecord>(patientsSql,
        new { userId, offset, pageSize });

    return (patients.ToList(), total);
  }

  public (IReadOnlyList<HandoverRecord> Handovers, int TotalCount) GetMyHandovers(string userId, int page, int pageSize)
  {
    using IDbConnection conn = _factory.CreateConnection();

    // First, get the patient IDs assigned to this user from database
    const string patientIdsSql = "SELECT PATIENT_ID FROM USER_ASSIGNMENTS WHERE USER_ID = :userId";
    var patientIds = conn.Query<string>(patientIdsSql, new { userId }).ToArray();

    if (patientIds.Length == 0)
      return (Array.Empty<HandoverRecord>(), 0);

    // Get total count of handovers for user's patients
    const string countSql = "SELECT COUNT(1) FROM HANDOVERS WHERE PATIENT_ID IN :patientIds";
    int total = conn.ExecuteScalar<int>(countSql, new { patientIds });

    if (total == 0)
      return (Array.Empty<HandoverRecord>(), 0);

    // Get handovers with pagination
    int p = Math.Max(page, 1);
    int ps = Math.Max(pageSize, 1);
    int offset = (p - 1) * ps;

    const string handoverSql = @"
      SELECT h.ID, h.ASSIGNMENT_ID, h.PATIENT_ID, p.NAME as PATIENT_NAME,
             h.STATUS,
             hpd.ILLNESS_SEVERITY,
             hpd.SUMMARY_TEXT as PATIENT_SUMMARY,
             hsyn.CONTENT as SYNTHESIS,
             h.SHIFT_NAME, h.CREATED_BY, h.TO_DOCTOR_ID as ASSIGNED_TO, h.RECEIVER_USER_ID, h.RESPONSIBLE_PHYSICIAN_ID,
             TO_CHAR(h.CREATED_AT, 'YYYY-MM-DD HH24:MI:SS') as CREATED_AT,
             TO_CHAR(h.ACKNOWLEDGED_AT, 'YYYY-MM-DD HH24:MI:SS') as ACKNOWLEDGED_AT,
             h.VERSION
      FROM HANDOVERS h
      LEFT JOIN HANDOVER_PATIENT_DATA hpd ON h.ID = hpd.HANDOVER_ID
      LEFT JOIN HANDOVER_SYNTHESIS hsyn ON h.ID = hsyn.HANDOVER_ID
      LEFT JOIN PATIENTS p ON h.PATIENT_ID = p.ID
      WHERE h.PATIENT_ID IN :patientIds
      ORDER BY h.CREATED_AT DESC
      OFFSET :offset ROWS FETCH NEXT :pageSize ROWS ONLY";

    var handoverRows = conn.Query(handoverSql, new { patientIds, offset, pageSize }).ToList();

    // Get action items for each handover
    var handovers = new List<HandoverRecord>();
    foreach (var row in handoverRows)
    {
      var handoverId = row.ID;


      var handover = new HandoverRecord(
        Id: row.ID,
        AssignmentId: row.ASSIGNMENT_ID ?? "",
        PatientId: row.PATIENT_ID,
        PatientName: row.PATIENT_NAME ?? "Unknown Patient",
        Status: row.STATUS,
        IllnessSeverity: new HandoverIllnessSeverity(row.ILLNESS_SEVERITY ?? "Stable"),
        PatientSummary: new HandoverPatientSummary(row.PATIENT_SUMMARY ?? ""),
        SituationAwarenessDocId: null,
        Synthesis: string.IsNullOrEmpty(row.SYNTHESIS) ? null : new HandoverSynthesis(row.SYNTHESIS),
        ShiftName: row.SHIFT_NAME ?? "Unknown",
        CreatedBy: row.CREATED_BY ?? "system",
        AssignedTo: row.ASSIGNED_TO ?? "system",
                    CreatedByName: null,
                    AssignedToName: null,
        ReceiverUserId: row.RECEIVER_USER_ID,
        ResponsiblePhysicianId: row.RESPONSIBLE_PHYSICIAN_ID ?? "",
        ResponsiblePhysicianName: null,
        CreatedAt: ((DateTime?)row.CREATED_AT)?.ToString("yyyy-MM-dd HH:mm:ss") ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
        ReadyAt: ((DateTime?)row.READY_AT)?.ToString("yyyy-MM-dd HH:mm:ss"),
        StartedAt: ((DateTime?)row.STARTED_AT)?.ToString("yyyy-MM-dd HH:mm:ss"),
        AcknowledgedAt: ((DateTime?)row.ACKNOWLEDGED_AT)?.ToString("yyyy-MM-dd HH:mm:ss"),
        AcceptedAt: ((DateTime?)row.ACCEPTED_AT)?.ToString("yyyy-MM-dd HH:mm:ss"),
        CompletedAt: ((DateTime?)row.COMPLETED_AT)?.ToString("yyyy-MM-dd HH:mm:ss"),
        CancelledAt: ((DateTime?)row.CANCELLED_AT)?.ToString("yyyy-MM-dd HH:mm:ss"),
        RejectedAt: ((DateTime?)row.REJECTED_AT)?.ToString("yyyy-MM-dd HH:mm:ss"),
        RejectionReason: row.REJECTION_REASON,
        ExpiredAt: ((DateTime?)row.EXPIRED_AT)?.ToString("yyyy-MM-dd HH:mm:ss"),
        HandoverType: row.HANDOVER_TYPE,
        HandoverWindowDate: row.HANDOVER_WINDOW_DATE,
        FromShiftId: row.FROM_SHIFT_ID,
        ToShiftId: row.TO_SHIFT_ID,
        ToDoctorId: row.TO_DOCTOR_ID,
        StateName: row.STATENAME ?? "Draft",
        Version: row.VERSION ?? 1
      );

      handovers.Add(handover);
    }

    return (handovers, total);
  }

  public HandoverRecord? GetHandoverById(string handoverId)
  {
    System.Diagnostics.Debug.WriteLine($"[OracleSetupDataProvider] Getting handover by ID: {handoverId}");
    try
    {
      using IDbConnection conn = _factory.CreateConnection();

    const string sql = @"
      SELECT h.ID, h.ASSIGNMENT_ID, h.PATIENT_ID, p.NAME as PATIENT_NAME,
             h.STATUS,
             hpd.ILLNESS_SEVERITY,
             hpd.SUMMARY_TEXT as PATIENT_SUMMARY,
             hsyn.CONTENT as SYNTHESIS,
             h.SHIFT_NAME, h.CREATED_BY, h.TO_DOCTOR_ID as ASSIGNED_TO, h.RECEIVER_USER_ID,
             COALESCE(h.RESPONSIBLE_PHYSICIAN_ID, h.CREATED_BY) AS RESPONSIBLE_PHYSICIAN_ID,
             TO_CHAR(h.CREATED_AT, 'YYYY-MM-DD HH24:MI:SS') as CREATED_AT,
             TO_CHAR(h.READY_AT, 'YYYY-MM-DD HH24:MI:SS') as READY_AT,
             TO_CHAR(h.STARTED_AT, 'YYYY-MM-DD HH24:MI:SS') as STARTED_AT,
             TO_CHAR(h.ACKNOWLEDGED_AT, 'YYYY-MM-DD HH24:MI:SS') as ACKNOWLEDGED_AT,
             TO_CHAR(h.ACCEPTED_AT, 'YYYY-MM-DD HH24:MI:SS') as ACCEPTED_AT,
             TO_CHAR(h.COMPLETED_AT, 'YYYY-MM-DD HH24:MI:SS') as COMPLETED_AT,
             TO_CHAR(h.CANCELLED_AT, 'YYYY-MM-DD HH24:MI:SS') as CANCELLED_AT,
             TO_CHAR(h.REJECTED_AT, 'YYYY-MM-DD HH24:MI:SS') as REJECTED_AT,
             h.REJECTION_REASON,
             TO_CHAR(h.EXPIRED_AT, 'YYYY-MM-DD HH24:MI:SS') as EXPIRED_AT,
             h.HANDOVER_TYPE,
             h.HANDOVER_WINDOW_DATE,
             h.FROM_SHIFT_ID,
             h.TO_SHIFT_ID,
             vws.StateName,
             h.VERSION
      FROM HANDOVERS h
      LEFT JOIN HANDOVER_PATIENT_DATA hpd ON h.ID = hpd.HANDOVER_ID
      LEFT JOIN HANDOVER_SYNTHESIS hsyn ON h.ID = hsyn.HANDOVER_ID
      LEFT JOIN PATIENTS p ON h.PATIENT_ID = p.ID
      LEFT JOIN VW_HANDOVERS_STATE vws ON h.ID = vws.HandoverId
      WHERE h.ID = :handoverId";

    var row = conn.QueryFirstOrDefault(sql, new { handoverId });

    if (row == null) return null;

    const string actionItemsSql = @"
      SELECT ID, DESCRIPTION, IS_COMPLETED
      FROM HANDOVER_ACTION_ITEMS
      WHERE HANDOVER_ID = :handoverId
      ORDER BY CREATED_AT";

    var actionItems = conn.Query(actionItemsSql, new { handoverId })
      .Select(item => new HandoverActionItem(item.ID, item.DESCRIPTION, item.IS_COMPLETED == 1))
      .ToList();

    // Safely extract values from dynamic row
    string? synthesisContent = row.SYNTHESIS as string;
    
    return new HandoverRecord(
      Id: (string)row.ID,
      AssignmentId: (row.ASSIGNMENT_ID as string) ?? "",
      PatientId: (string)row.PATIENT_ID,
      PatientName: (row.PATIENT_NAME as string) ?? "Unknown Patient",
      Status: (string)row.STATUS,
      IllnessSeverity: new HandoverIllnessSeverity((row.ILLNESS_SEVERITY as string) ?? "Stable"),
      PatientSummary: new HandoverPatientSummary((row.PATIENT_SUMMARY as string) ?? ""),
      SituationAwarenessDocId: null,
      Synthesis: !string.IsNullOrEmpty(synthesisContent) ? new HandoverSynthesis(synthesisContent) : null,
      ShiftName: (row.SHIFT_NAME as string) ?? "Unknown",
      CreatedBy: (row.CREATED_BY as string) ?? "system",
      AssignedTo: (row.ASSIGNED_TO as string) ?? "system",
      CreatedByName: null,
      AssignedToName: null,
      ReceiverUserId: row.RECEIVER_USER_ID as string,
      ResponsiblePhysicianId: (row.RESPONSIBLE_PHYSICIAN_ID as string) ?? "system",
      ResponsiblePhysicianName: "Unknown", // Default value since we don't have user names in this query
      CreatedAt: row.CREATED_AT as string,
      ReadyAt: row.READY_AT as string,
      StartedAt: row.STARTED_AT as string,
      AcknowledgedAt: row.ACKNOWLEDGED_AT as string,
      AcceptedAt: row.ACCEPTED_AT as string,
      CompletedAt: row.COMPLETED_AT as string,
      CancelledAt: row.CANCELLED_AT as string,
      RejectedAt: row.REJECTED_AT as string,
      RejectionReason: row.REJECTION_REASON as string,
      ExpiredAt: row.EXPIRED_AT as string,
      HandoverType: (row.HANDOVER_TYPE as string) ?? "ShiftToShift",
      HandoverWindowDate: row.HANDOVER_WINDOW_DATE as DateTime?,
      FromShiftId: row.FROM_SHIFT_ID as string,
      ToShiftId: row.TO_SHIFT_ID as string,
      ToDoctorId: row.TO_DOCTOR_ID as string,
      StateName: (row.STATENAME as string) ?? "Draft",
      Version: (row.VERSION as int?) ?? 1
    );
    }
    catch (Exception ex)
    {
      System.Diagnostics.Debug.WriteLine($"[OracleSetupDataProvider] Error getting handover by ID {handoverId}: {ex.Message}");
      throw;
    }
  }

  public async Task<HandoverRecord> CreateHandoverAsync(CreateHandoverRequest request)
  {
    await Task.CompletedTask; // Make async
    using IDbConnection conn = _factory.CreateConnection();

    // Generate IDs
    var handoverId = $"handover-{Guid.NewGuid().ToString().Substring(0, 8)}";
    var assignmentId = $"assign-{Guid.NewGuid().ToString().Substring(0, 8)}";

    // Create user assignment if it doesn't exist (using MERGE for Oracle)
    conn.Execute(@"
      MERGE INTO USER_ASSIGNMENTS ua
      USING (SELECT :assignmentId AS ASSIGNMENT_ID, :toDoctorId AS USER_ID, :toShiftId AS SHIFT_ID, :patientId AS PATIENT_ID FROM DUAL) src
      ON (ua.ASSIGNMENT_ID = src.ASSIGNMENT_ID)
      WHEN MATCHED THEN
        UPDATE SET ua.ASSIGNED_AT = SYSTIMESTAMP
      WHEN NOT MATCHED THEN
        INSERT (ASSIGNMENT_ID, USER_ID, SHIFT_ID, PATIENT_ID, ASSIGNED_AT)
        VALUES (src.ASSIGNMENT_ID, src.USER_ID, src.SHIFT_ID, src.PATIENT_ID, SYSTIMESTAMP)",
      new { assignmentId, toDoctorId = request.ToDoctorId, toShiftId = request.ToShiftId, patientId = request.PatientId });

    // Create handover
    conn.Execute(@"
      INSERT INTO HANDOVERS (
        ID, ASSIGNMENT_ID, PATIENT_ID, STATUS,
        SHIFT_NAME, FROM_SHIFT_ID, TO_SHIFT_ID, FROM_DOCTOR_ID, TO_DOCTOR_ID,
        CREATED_BY, CREATED_AT, INITIATED_AT, HANDOVER_TYPE, HANDOVER_WINDOW_DATE
      ) VALUES (
        :handoverId, :assignmentId, :patientId, 'Draft',
        :shiftName, :fromShiftId, :toShiftId, :fromDoctorId, :toDoctorId,
        :initiatedBy, SYSTIMESTAMP, SYSTIMESTAMP, 'ShiftToShift', TRUNC(SYSDATE)
      )",
      new {
        handoverId,
        assignmentId,
        patientId = request.PatientId,
        shiftName = $"{request.FromShiftId} → {request.ToShiftId}",
        fromShiftId = request.FromShiftId,
        toShiftId = request.ToShiftId,
        fromDoctorId = request.FromDoctorId,
        toDoctorId = request.ToDoctorId,
        initiatedBy = request.InitiatedBy
      });

    // Add participants (with default names if users don't exist)
    conn.Execute(@"
      INSERT INTO HANDOVER_PARTICIPANTS (ID, HANDOVER_ID, USER_ID, USER_NAME, USER_ROLE, STATUS, JOINED_AT)
      VALUES (:participantId1, :handoverId, :fromDoctorId, 'Doctor A', 'Handing Over Doctor', 'active', SYSTIMESTAMP)",
      new {
        handoverId,
        fromDoctorId = request.FromDoctorId,
        participantId1 = $"participant-{Guid.NewGuid().ToString().Substring(0, 8)}"
      });
    
    conn.Execute(@"
      INSERT INTO HANDOVER_PARTICIPANTS (ID, HANDOVER_ID, USER_ID, USER_NAME, USER_ROLE, STATUS, JOINED_AT)
      VALUES (:participantId2, :handoverId, :toDoctorId, 'Doctor B', 'Receiving Doctor', 'active', SYSTIMESTAMP)",
      new {
        handoverId,
        toDoctorId = request.ToDoctorId,
        participantId2 = $"participant-{Guid.NewGuid().ToString().Substring(0, 8)}"
      });

    // Create default singleton sections
    conn.Execute(@"
      INSERT INTO HANDOVER_PATIENT_DATA (HANDOVER_ID, ILLNESS_SEVERITY, SUMMARY_TEXT, STATUS, LAST_EDITED_BY, CREATED_AT, UPDATED_AT)
      VALUES (:handoverId, 'Stable', '', 'draft', :initiatedBy, SYSTIMESTAMP, SYSTIMESTAMP)",
      new { handoverId, initiatedBy = request.InitiatedBy });

    conn.Execute(@"
      INSERT INTO HANDOVER_SITUATION_AWARENESS (HANDOVER_ID, CONTENT, STATUS, LAST_EDITED_BY, CREATED_AT, UPDATED_AT)
      VALUES (:handoverId, '', 'draft', :initiatedBy, SYSTIMESTAMP, SYSTIMESTAMP)",
      new { handoverId, initiatedBy = request.InitiatedBy });

    conn.Execute(@"
      INSERT INTO HANDOVER_SYNTHESIS (HANDOVER_ID, CONTENT, STATUS, LAST_EDITED_BY, CREATED_AT, UPDATED_AT)
      VALUES (:handoverId, '', 'draft', :initiatedBy, SYSTIMESTAMP, SYSTIMESTAMP)",
      new { handoverId, initiatedBy = request.InitiatedBy });

    // Return the created handover
    return GetHandoverById(handoverId) ?? throw new InvalidOperationException("Failed to create handover");
  }

  public async Task<bool> StartHandoverAsync(string handoverId, string userId)
  {
    await Task.CompletedTask;
    using IDbConnection conn = _factory.CreateConnection();

    var affected = conn.Execute(@"
      UPDATE HANDOVERS
      SET STARTED_AT = SYSTIMESTAMP, STATUS = 'InProgress', UPDATED_AT = SYSTIMESTAMP
      WHERE ID = :handoverId AND READY_AT IS NOT NULL AND STARTED_AT IS NULL",
      new { handoverId, userId });

    return affected > 0;
  }

  public async Task<bool> AcceptHandoverAsync(string handoverId, string userId)
  {
    await Task.CompletedTask; // Make async
    using IDbConnection conn = _factory.CreateConnection();

    var affected = conn.Execute(@"
      UPDATE HANDOVERS
      SET ACCEPTED_AT = SYSTIMESTAMP
      WHERE ID = :handoverId AND STARTED_AT IS NOT NULL AND ACCEPTED_AT IS NULL",
      new { handoverId, userId });

    return affected > 0;
  }

  public async Task<bool> CompleteHandoverAsync(string handoverId, string userId)
  {
    await Task.CompletedTask; // Make async
    using IDbConnection conn = _factory.CreateConnection();

    var affected = conn.Execute(@"
      UPDATE HANDOVERS
      SET STATUS = 'Completed', COMPLETED_AT = SYSTIMESTAMP, COMPLETED_BY = :userId, UPDATED_AT = SYSTIMESTAMP
      WHERE ID = :handoverId AND ACCEPTED_AT IS NOT NULL AND COMPLETED_AT IS NULL",
      new { handoverId, userId });

    return affected > 0;
  }

  public async Task<IReadOnlyList<HandoverRecord>> GetPendingHandoversForUserAsync(string userId)
  {
    await Task.CompletedTask; // Make async
    using IDbConnection conn = _factory.CreateConnection();

    const string sql = @"
      SELECT h.ID, h.ASSIGNMENT_ID, h.PATIENT_ID, p.NAME as PATIENT_NAME,
             h.STATUS, hpd.ILLNESS_SEVERITY, hpd.SUMMARY_TEXT as PATIENT_SUMMARY, hs.CONTENT as SYNTHESIS,
             h.SHIFT_NAME, h.CREATED_BY, h.TO_DOCTOR_ID as ASSIGNED_TO, h.RECEIVER_USER_ID,
             TO_CHAR(h.CREATED_AT, 'YYYY-MM-DD HH24:MI:SS') as CREATED_AT,
             TO_CHAR(h.ACKNOWLEDGED_AT, 'YYYY-MM-DD HH24:MI:SS') as ACKNOWLEDGED_AT,
             h.VERSION
      FROM HANDOVERS h
      INNER JOIN PATIENTS p ON h.PATIENT_ID = p.ID
      LEFT JOIN HANDOVER_PATIENT_DATA hpd ON h.ID = hpd.HANDOVER_ID
      LEFT JOIN HANDOVER_SYNTHESIS hs ON h.ID = hs.HANDOVER_ID
      WHERE h.TO_DOCTOR_ID = :userId
        AND h.READY_AT IS NOT NULL
        AND h.ACCEPTED_AT IS NULL
        AND h.COMPLETED_AT IS NULL
        AND h.CANCELLED_AT IS NULL
        AND h.REJECTED_AT IS NULL
        AND h.EXPIRED_AT IS NULL
      ORDER BY h.INITIATED_AT DESC";

    var handoverRows = conn.Query(sql, new { userId }).ToList();
    var handovers = new List<HandoverRecord>();

    foreach (var row in handoverRows)
    {
      var handoverId = row.ID;


      var handover = new HandoverRecord(
        Id: row.ID,
        AssignmentId: row.ASSIGNMENT_ID ?? "",
        PatientId: row.PATIENT_ID,
        PatientName: row.PATIENT_NAME ?? "Unknown Patient",
        Status: row.STATUS,
        IllnessSeverity: new HandoverIllnessSeverity(row.ILLNESS_SEVERITY ?? "Stable"),
        PatientSummary: new HandoverPatientSummary(row.PATIENT_SUMMARY ?? ""),
        SituationAwarenessDocId: null,
        Synthesis: string.IsNullOrEmpty(row.SYNTHESIS) ? null : new HandoverSynthesis(row.SYNTHESIS),
        ShiftName: row.SHIFT_NAME ?? "Unknown",
        CreatedBy: row.CREATED_BY ?? "system",
        AssignedTo: row.ASSIGNED_TO ?? "system",
                    CreatedByName: null,
                    AssignedToName: null,
        ReceiverUserId: row.RECEIVER_USER_ID,
        ResponsiblePhysicianId: row.RESPONSIBLE_PHYSICIAN_ID ?? "",
        ResponsiblePhysicianName: null,
        CreatedAt: ((DateTime?)row.CREATED_AT)?.ToString("yyyy-MM-dd HH:mm:ss") ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
        ReadyAt: ((DateTime?)row.READY_AT)?.ToString("yyyy-MM-dd HH:mm:ss"),
        StartedAt: ((DateTime?)row.STARTED_AT)?.ToString("yyyy-MM-dd HH:mm:ss"),
        AcknowledgedAt: ((DateTime?)row.ACKNOWLEDGED_AT)?.ToString("yyyy-MM-dd HH:mm:ss"),
        AcceptedAt: ((DateTime?)row.ACCEPTED_AT)?.ToString("yyyy-MM-dd HH:mm:ss"),
        CompletedAt: ((DateTime?)row.COMPLETED_AT)?.ToString("yyyy-MM-dd HH:mm:ss"),
        CancelledAt: ((DateTime?)row.CANCELLED_AT)?.ToString("yyyy-MM-dd HH:mm:ss"),
        RejectedAt: ((DateTime?)row.REJECTED_AT)?.ToString("yyyy-MM-dd HH:mm:ss"),
        RejectionReason: row.REJECTION_REASON,
        ExpiredAt: ((DateTime?)row.EXPIRED_AT)?.ToString("yyyy-MM-dd HH:mm:ss"),
        HandoverType: row.HANDOVER_TYPE,
        HandoverWindowDate: row.HANDOVER_WINDOW_DATE,
        FromShiftId: row.FROM_SHIFT_ID,
        ToShiftId: row.TO_SHIFT_ID,
        ToDoctorId: row.TO_DOCTOR_ID,
        StateName: row.STATENAME ?? "Draft",
        Version: row.VERSION ?? 1
      );

      handovers.Add(handover);
    }

    return handovers;
  }

  public async Task<IReadOnlyList<HandoverRecord>> GetHandoversByPatientAsync(string patientId)
  {
    await Task.CompletedTask; // Make async
    using IDbConnection conn = _factory.CreateConnection();

    const string sql = @"
      SELECT h.ID, h.ASSIGNMENT_ID, h.PATIENT_ID, p.NAME as PATIENT_NAME,
             h.STATUS,
             hpd.ILLNESS_SEVERITY,
             hpd.SUMMARY_TEXT as PATIENT_SUMMARY,
             hsyn.CONTENT as SYNTHESIS,
             h.SHIFT_NAME, h.CREATED_BY, h.TO_DOCTOR_ID as ASSIGNED_TO, h.RECEIVER_USER_ID,
             TO_CHAR(h.CREATED_AT, 'YYYY-MM-DD HH24:MI:SS') as CREATED_AT,
             TO_CHAR(h.ACKNOWLEDGED_AT, 'YYYY-MM-DD HH24:MI:SS') as ACKNOWLEDGED_AT,
             h.VERSION
      FROM HANDOVERS h
      INNER JOIN PATIENTS p ON h.PATIENT_ID = p.ID
      LEFT JOIN HANDOVER_PATIENT_DATA hpd ON h.ID = hpd.HANDOVER_ID
      LEFT JOIN HANDOVER_SYNTHESIS hsyn ON h.ID = hsyn.HANDOVER_ID
      WHERE h.PATIENT_ID = :patientId
      ORDER BY h.CREATED_AT DESC";

    var handoverRows = conn.Query(sql, new { patientId }).ToList();
    var handovers = new List<HandoverRecord>();

    foreach (var row in handoverRows)
    {
      var handoverId = row.ID;


      var handover = new HandoverRecord(
        Id: row.ID,
        AssignmentId: row.ASSIGNMENT_ID ?? "",
        PatientId: row.PATIENT_ID,
        PatientName: row.PATIENT_NAME ?? "Unknown Patient",
        Status: row.STATUS,
        IllnessSeverity: new HandoverIllnessSeverity(row.ILLNESS_SEVERITY ?? "Stable"),
        PatientSummary: new HandoverPatientSummary(row.PATIENT_SUMMARY ?? ""),
        SituationAwarenessDocId: null,
        Synthesis: string.IsNullOrEmpty(row.SYNTHESIS) ? null : new HandoverSynthesis(row.SYNTHESIS),
        ShiftName: row.SHIFT_NAME ?? "Unknown",
        CreatedBy: row.CREATED_BY ?? "system",
        AssignedTo: row.ASSIGNED_TO ?? "system",
                    CreatedByName: null,
                    AssignedToName: null,
        ReceiverUserId: row.RECEIVER_USER_ID,
        ResponsiblePhysicianId: row.RESPONSIBLE_PHYSICIAN_ID ?? "",
        ResponsiblePhysicianName: null,
        CreatedAt: ((DateTime?)row.CREATED_AT)?.ToString("yyyy-MM-dd HH:mm:ss") ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
        ReadyAt: ((DateTime?)row.READY_AT)?.ToString("yyyy-MM-dd HH:mm:ss"),
        StartedAt: ((DateTime?)row.STARTED_AT)?.ToString("yyyy-MM-dd HH:mm:ss"),
        AcknowledgedAt: ((DateTime?)row.ACKNOWLEDGED_AT)?.ToString("yyyy-MM-dd HH:mm:ss"),
        AcceptedAt: ((DateTime?)row.ACCEPTED_AT)?.ToString("yyyy-MM-dd HH:mm:ss"),
        CompletedAt: ((DateTime?)row.COMPLETED_AT)?.ToString("yyyy-MM-dd HH:mm:ss"),
        CancelledAt: ((DateTime?)row.CANCELLED_AT)?.ToString("yyyy-MM-dd HH:mm:ss"),
        RejectedAt: ((DateTime?)row.REJECTED_AT)?.ToString("yyyy-MM-dd HH:mm:ss"),
        RejectionReason: row.REJECTION_REASON,
        ExpiredAt: ((DateTime?)row.EXPIRED_AT)?.ToString("yyyy-MM-dd HH:mm:ss"),
        HandoverType: row.HANDOVER_TYPE,
        HandoverWindowDate: row.HANDOVER_WINDOW_DATE,
        FromShiftId: row.FROM_SHIFT_ID,
        ToShiftId: row.TO_SHIFT_ID,
        ToDoctorId: row.TO_DOCTOR_ID,
        StateName: row.STATENAME ?? "Draft",
        Version: row.VERSION ?? 1
      );

      handovers.Add(handover);
    }

    return handovers;
  }

  public async Task<IReadOnlyList<HandoverRecord>> GetShiftTransitionHandoversAsync(string fromDoctorId, string toDoctorId)
  {
    await Task.CompletedTask; // Make async
    using IDbConnection conn = _factory.CreateConnection();

    const string sql = @"
      SELECT h.ID, h.ASSIGNMENT_ID, h.PATIENT_ID, p.NAME as PATIENT_NAME,
             h.STATUS,
             hpd.ILLNESS_SEVERITY,
             hpd.SUMMARY_TEXT as PATIENT_SUMMARY,
             hsyn.CONTENT as SYNTHESIS,
             h.SHIFT_NAME, h.CREATED_BY, h.TO_DOCTOR_ID as ASSIGNED_TO, h.RECEIVER_USER_ID,
             TO_CHAR(h.CREATED_AT, 'YYYY-MM-DD HH24:MI:SS') as CREATED_AT,
             TO_CHAR(h.ACKNOWLEDGED_AT, 'YYYY-MM-DD HH24:MI:SS') as ACKNOWLEDGED_AT,
             h.VERSION
      FROM HANDOVERS h
      INNER JOIN PATIENTS p ON h.PATIENT_ID = p.ID
      LEFT JOIN HANDOVER_PATIENT_DATA hpd ON h.ID = hpd.HANDOVER_ID
      LEFT JOIN HANDOVER_SYNTHESIS hsyn ON h.ID = hsyn.HANDOVER_ID
      WHERE h.FROM_DOCTOR_ID = :fromDoctorId AND h.TO_DOCTOR_ID = :toDoctorId
      ORDER BY h.INITIATED_AT DESC";

    var handoverRows = conn.Query(sql, new { fromDoctorId, toDoctorId }).ToList();
    var handovers = new List<HandoverRecord>();

    foreach (var row in handoverRows)
    {
      var handoverId = row.ID;


      var handover = new HandoverRecord(
        Id: row.ID,
        AssignmentId: row.ASSIGNMENT_ID ?? "",
        PatientId: row.PATIENT_ID,
        PatientName: row.PATIENT_NAME ?? "Unknown Patient",
        Status: row.STATUS,
        IllnessSeverity: new HandoverIllnessSeverity(row.ILLNESS_SEVERITY ?? "Stable"),
        PatientSummary: new HandoverPatientSummary(row.PATIENT_SUMMARY ?? ""),
        SituationAwarenessDocId: null,
        Synthesis: string.IsNullOrEmpty(row.SYNTHESIS) ? null : new HandoverSynthesis(row.SYNTHESIS),
        ShiftName: row.SHIFT_NAME ?? "Unknown",
        CreatedBy: row.CREATED_BY ?? "system",
        AssignedTo: row.ASSIGNED_TO ?? "system",
                    CreatedByName: null,
                    AssignedToName: null,
        ReceiverUserId: row.RECEIVER_USER_ID,
        ResponsiblePhysicianId: row.RESPONSIBLE_PHYSICIAN_ID ?? "",
        ResponsiblePhysicianName: null,
        CreatedAt: ((DateTime?)row.CREATED_AT)?.ToString("yyyy-MM-dd HH:mm:ss") ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
        ReadyAt: ((DateTime?)row.READY_AT)?.ToString("yyyy-MM-dd HH:mm:ss"),
        StartedAt: ((DateTime?)row.STARTED_AT)?.ToString("yyyy-MM-dd HH:mm:ss"),
        AcknowledgedAt: ((DateTime?)row.ACKNOWLEDGED_AT)?.ToString("yyyy-MM-dd HH:mm:ss"),
        AcceptedAt: ((DateTime?)row.ACCEPTED_AT)?.ToString("yyyy-MM-dd HH:mm:ss"),
        CompletedAt: ((DateTime?)row.COMPLETED_AT)?.ToString("yyyy-MM-dd HH:mm:ss"),
        CancelledAt: ((DateTime?)row.CANCELLED_AT)?.ToString("yyyy-MM-dd HH:mm:ss"),
        RejectedAt: ((DateTime?)row.REJECTED_AT)?.ToString("yyyy-MM-dd HH:mm:ss"),
        RejectionReason: row.REJECTION_REASON,
        ExpiredAt: ((DateTime?)row.EXPIRED_AT)?.ToString("yyyy-MM-dd HH:mm:ss"),
        HandoverType: row.HANDOVER_TYPE,
        HandoverWindowDate: row.HANDOVER_WINDOW_DATE,
        FromShiftId: row.FROM_SHIFT_ID,
        ToShiftId: row.TO_SHIFT_ID,
        ToDoctorId: row.TO_DOCTOR_ID,
        StateName: row.STATENAME ?? "Draft",
        Version: row.VERSION ?? 1
      );

      handovers.Add(handover);
    }

    return handovers;
  }
}


