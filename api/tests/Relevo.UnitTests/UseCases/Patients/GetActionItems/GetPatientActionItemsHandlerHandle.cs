using Ardalis.Result;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;
using Relevo.UseCases.Patients.GetActionItems;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Relevo.UnitTests.UseCases.Patients.GetActionItems;

public class GetPatientActionItemsHandlerHandle
{
    private readonly IPatientRepository _repository = Substitute.For<IPatientRepository>();
    private readonly GetPatientActionItemsHandler _handler;

    public GetPatientActionItemsHandlerHandle()
    {
        _handler = new GetPatientActionItemsHandler(_repository);
    }

    [Fact]
    public async Task ReturnsActionItems()
    {
        var patientId = "pat-1";
        var items = new List<PatientActionItemRecord>
        {
            new PatientActionItemRecord("ai-1", "hvo-1", "Check meds", false, DateTime.UtcNow, "dr-1", "Day")
        };

        _repository.GetPatientActionItemsAsync(patientId)
            .Returns(Task.FromResult<IReadOnlyList<PatientActionItemRecord>>(items));

        var result = await _handler.Handle(new GetPatientActionItemsQuery(patientId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }
}

