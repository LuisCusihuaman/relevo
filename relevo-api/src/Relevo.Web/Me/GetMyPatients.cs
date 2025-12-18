using FastEndpoints;
using MediatR;
using Relevo.Core.Interfaces;
using Relevo.UseCases.Me.Patients;

namespace Relevo.Web.Me;

public record GetMyPatientsRequest
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 25;
}

public record PatientSummaryCard
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string HandoverStatus { get; init; } = "not-started";
    public string? HandoverId { get; init; }
    public string? Severity { get; init; }
}

public record GetMyPatientsResponse
{
    public IReadOnlyList<PatientSummaryCard> Items { get; init; } = [];
    public PatientsPaginationInfo Pagination { get; init; } = new();
}

public record PatientsPaginationInfo
{
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
}

public class GetMyPatientsEndpoint(IMediator _mediator, ICurrentUser _currentUser)
    : Endpoint<GetMyPatientsRequest, GetMyPatientsResponse>
{
    public override void Configure()
    {
        Get("/me/patients");
    }

    public override async Task HandleAsync(GetMyPatientsRequest req, CancellationToken ct)
    {
        var userId = _currentUser.Id;
        if (string.IsNullOrEmpty(userId))
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var page = req.Page <= 0 ? 1 : req.Page;
        var pageSize = req.PageSize <= 0 ? 25 : req.PageSize;

        var result = await _mediator.Send(new GetMyPatientsQuery(userId, page, pageSize), ct);

        if (!result.IsSuccess)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        var (patients, total) = result.Value;

        Response = new GetMyPatientsResponse
        {
            Items = patients.Select(p => new PatientSummaryCard
            {
                Id = p.Id,
                Name = p.Name,
                HandoverStatus = p.HandoverStatus,
                HandoverId = p.HandoverId,
                Severity = p.Severity
            }).ToList(),
            Pagination = new PatientsPaginationInfo
            {
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            }
        };
        await SendAsync(Response, cancellation: ct);
    }
}

