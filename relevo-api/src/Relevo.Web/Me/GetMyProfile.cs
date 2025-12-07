using FastEndpoints;
using MediatR;
using Relevo.Core.Interfaces;
using Relevo.UseCases.Me.Profile;
using System.ComponentModel.DataAnnotations;

namespace Relevo.Web.Me;

public record GetMyProfileResponse
{
    [Required]
    public required string Id { get; init; }
    [Required]
    public required string Email { get; init; }
    [Required]
    public required string FirstName { get; init; }
    [Required]
    public required string LastName { get; init; }
    [Required]
    public required string FullName { get; init; }
    [Required]
    public required IReadOnlyList<string> Roles { get; init; }
    [Required]
    public required bool IsActive { get; init; }
}

public class GetMyProfileEndpoint(IMediator _mediator, ICurrentUser _currentUser)
    : EndpointWithoutRequest<GetMyProfileResponse>
{
    public override void Configure()
    {
        Get("/me/profile");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = _currentUser.Id;
        if (string.IsNullOrEmpty(userId))
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

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

