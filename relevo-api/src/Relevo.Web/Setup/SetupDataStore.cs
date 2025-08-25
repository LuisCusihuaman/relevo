using Relevo.Web.Patients;

namespace Relevo.Web.Setup;

public class SetupDataStore
{
  private readonly List<UnitRecord> _units =
  [
    new("unit-1", "UCI"),
    new("unit-2", "Pediatría General"),
    new("unit-3", "Pediatría Especializada")
  ];

  private readonly List<ShiftRecord> _shifts =
  [
    new("shift-day", "Mañana", "07:00", "15:00"),
    new("shift-night", "Noche", "19:00", "07:00")
  ];

  private readonly Dictionary<string, List<PatientRecord>> _unitIdToPatients = new()
  {
    ["unit-1"] =
    [
      new("pat-123", "John Doe"),
      new("pat-456", "Jane Smith"),
      new("pat-789", "Alex Johnson")
    ],
    ["unit-2"] =
    [
      new("pat-210", "Ava Thompson"),
      new("pat-220", "Liam Rodríguez"),
      new("pat-230", "Mia Patel")
    ],
    ["unit-3"] =
    [
      new("pat-310", "Pat Taylor"),
      new("pat-320", "Jordan White")
    ]
  };

  // simple in-memory assignments per user id
  private readonly Dictionary<string, (string ShiftId, HashSet<string> PatientIds)> _assignments = new();

  public IReadOnlyList<UnitRecord> GetUnits() => _units;

  public IReadOnlyList<ShiftRecord> GetShifts() => _shifts;

  public (IReadOnlyList<PatientRecord> Patients, int TotalCount) GetPatientsByUnit(
    string unitId,
    int page,
    int pageSize)
  {
    if (!_unitIdToPatients.TryGetValue(unitId, out var patients))
    {
      patients = [];
    }

    var total = patients.Count;
    var items = patients
      .Skip((Math.Max(page, 1) - 1) * Math.Max(pageSize, 1))
      .Take(Math.Max(pageSize, 1))
      .ToList();

    return (items, total);
  }

  public void Assign(string userId, string shiftId, IEnumerable<string> patientIds)
  {
    if (!_assignments.TryGetValue(userId, out var existing))
    {
      existing = (shiftId, new HashSet<string>());
    }

    existing.ShiftId = shiftId;
    existing.PatientIds = new HashSet<string>(patientIds);
    _assignments[userId] = existing;
  }

  public (IReadOnlyList<PatientRecord> Patients, int TotalCount) GetMyPatients(
    string userId,
    int page,
    int pageSize)
  {
    if (!_assignments.TryGetValue(userId, out var assignment) || assignment.PatientIds.Count == 0)
    {
      return (Array.Empty<PatientRecord>(), 0);
    }

    // find patients across all units that match the ids
    var allPatients = _unitIdToPatients.Values.SelectMany(x => x).ToList();
    var selected = allPatients.Where(p => assignment.PatientIds.Contains(p.Id)).ToList();

    var total = selected.Count;
    var items = selected
      .Skip((Math.Max(page, 1) - 1) * Math.Max(pageSize, 1))
      .Take(Math.Max(pageSize, 1))
      .ToList();

    return (items, total);
  }
}

public record UnitRecord(string Id, string Name);

public record ShiftRecord(string Id, string Name, string StartTime, string EndTime);


