using Relevo.Core.Interfaces;

namespace Relevo.UseCases.Setup;

public class GetPatientByIdUseCase
{
    private readonly ISetupRepository _repository;

    public GetPatientByIdUseCase(ISetupRepository repository)
    {
        _repository = repository;
    }

    public PatientDetailRecord? Execute(string patientId)
    {
        return _repository.GetPatientById(patientId);
    }
}
