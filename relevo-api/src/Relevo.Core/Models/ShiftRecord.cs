using System.ComponentModel.DataAnnotations;

namespace Relevo.Core.Models;

public record ShiftRecord(
    [property: Required] string Id,
    [property: Required] string Name,
    [property: Required] string StartTime,
    [property: Required] string EndTime
);

