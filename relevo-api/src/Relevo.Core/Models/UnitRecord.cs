using System.ComponentModel.DataAnnotations;

namespace Relevo.Core.Models;

public record UnitRecord(
    [property: Required] string Id,
    [property: Required] string Name
);

