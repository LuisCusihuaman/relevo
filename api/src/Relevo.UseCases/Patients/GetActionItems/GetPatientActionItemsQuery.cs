using Ardalis.Result;
using Ardalis.SharedKernel;
using Relevo.Core.Models;

namespace Relevo.UseCases.Patients.GetActionItems;

public record GetPatientActionItemsQuery(string PatientId) : IQuery<Result<IReadOnlyList<PatientActionItemRecord>>>;

