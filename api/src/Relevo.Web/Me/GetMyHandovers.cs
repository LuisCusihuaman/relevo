using FastEndpoints;
using MediatR;
using Relevo.Core.Models;
using Relevo.UseCases.Me.Handovers;

namespace Relevo.Web.Me;

public record GetMyHandoversRequest
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 25;
}

public record GetMyHandoversResponse
{
    public IReadOnlyList<HandoverRecord> Items { get; init; } = [];
    public PaginationInfo Pagination { get; init; } = new();
}

public record PaginationInfo
{
    public int TotalItems { get; init; }
    public int CurrentPage { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
}

public class GetMyHandoversEndpoint(IMediator _mediator)
    : Endpoint<GetMyHandoversRequest, GetMyHandoversResponse>
{
    public override void Configure()
    {
        Get("/me/handovers");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetMyHandoversRequest req, CancellationToken ct)
    {
        // TODO: Get actual user ID from authentication context
        var userId = "dr-1";

        var page = req.Page <= 0 ? 1 : req.Page;
        var pageSize = req.PageSize <= 0 ? 25 : req.PageSize;

        var result = await _mediator.Send(new GetMyHandoversQuery(userId, page, pageSize), ct);

        if (!result.IsSuccess)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        var (handovers, total) = result.Value;

        Response = new GetMyHandoversResponse
        {
            Items = handovers,
            Pagination = new PaginationInfo
            {
                TotalItems = total,
                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)total / pageSize)
            }
        };
        await SendAsync(Response, cancellation: ct);
    }
}

