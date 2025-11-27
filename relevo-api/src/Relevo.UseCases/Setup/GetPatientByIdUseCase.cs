using System.Threading.Tasks;
using Relevo.Core.Interfaces;

namespace Relevo.UseCases.Setup;

public class GetPatientByIdUseCase
{
    private readonly IPatientRepository _repository;

    public GetPatientByIdUseCase(IPatientRepository repository)
    {
        _repository = repository;
    }

    public PatientDetailRecord? Execute(string patientId)
    {
        return _repository.GetPatientById(patientId);
    }
}
