using FastEndpoints;
using MediatR;
using Relevo.UseCases.Me.Profile;

namespace Relevo.Web.Me;

public record GetMyProfileResponse
{
    public string Id { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public IReadOnlyList<string> Roles { get; init; } = [];
    public bool IsActive { get; init; } = true;
}

public class GetMyProfileEndpoint(IMediator _mediator)
    : EndpointWithoutRequest<GetMyProfileResponse>
{
    public override void Configure()
    {
        Get("/me/profile");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // TODO: Get actual user ID from authentication context
        var userId = "dr-1";

        var result = await _mediator.Send(new GetMyProfileQuery(userId), ct);

        if (!result.IsSuccess || result.Value == null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        var user = result.Value;

        Response = new GetMyProfileResponse
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            Roles = user.Roles,
            IsActive = user.IsActive
        };
        await SendAsync(Response, cancellation: ct);
    }
}

