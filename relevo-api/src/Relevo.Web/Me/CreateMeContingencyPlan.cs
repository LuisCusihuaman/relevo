using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Relevo.Core.Interfaces;
using Relevo.Core.Models;
using Relevo.UseCases.Me.ContingencyPlans;

namespace Relevo.Web.Me;

public record CreateMeContingencyPlanRequest
{
    [FromRoute]
    public string HandoverId { get; init; } = string.Empty;

    public string ConditionText { get; init; } = string.Empty;
    public string ActionText { get; init; } = string.Empty;
    public string Priority { get; init; } = "medium"; // V3: Must be lowercase per CHK_CONT_PRIORITY constraint
}

public record CreateMeContingencyPlanResponse
{
    public bool Success { get; init; }
    public ContingencyPlanRecord? ContingencyPlan { get; init; }
}

public class CreateMeContingencyPlanEndpoint(IMediator _mediator, ICurrentUser _currentUser, IUserRepository _userRepository)
    : Endpoint<CreateMeContingencyPlanRequest, CreateMeContingencyPlanResponse>
{
    public override void Configure()
    {
        Post("/me/handovers/{handoverId}/contingency-plans");
    }

    public override async Task HandleAsync(CreateMeContingencyPlanRequest req, CancellationToken ct)
    {
        var userId = _currentUser.Id;
        if (string.IsNullOrEmpty(userId))
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        // Ensure user exists in database before creating contingency plan
        // This ensures the FK constraint is satisfied
        await _userRepository.EnsureUserExistsAsync(
            userId,
            _currentUser.Email,
            _currentUser.FirstName,
            _currentUser.LastName,
            _currentUser.FullName,
            _currentUser.AvatarUrl,
            _currentUser.OrgRole
        );

        try
        {
            // Normalize priority to lowercase for constraint compliance
            var normalizedPriority = req.Priority.ToLowerInvariant();
            if (normalizedPriority != "low" && normalizedPriority != "medium" && normalizedPriority != "high")
            {
                normalizedPriority = "medium"; // Default to medium if invalid
            }

            var result = await _mediator.Send(
                new CreateMeContingencyPlanCommand(
                    req.HandoverId,
                    req.ConditionText,
                    req.ActionText,
                    normalizedPriority,
                    userId), // Use user ID, not FullName (FK constraint requires USERS.ID)
                ct);

            if (!result.IsSuccess)
            {
                var errorMessage = result.Errors.FirstOrDefault() ?? "Error creating contingency plan";
                AddError(errorMessage);
                await SendErrorsAsync(statusCode: 400, cancellation: ct);
                return;
            }

            Response = new CreateMeContingencyPlanResponse
            {
                Success = true,
                ContingencyPlan = result.Value
            };
            await SendAsync(Response, cancellation: ct);
        }
        catch (Exception ex)
        {
            AddError($"Error creating contingency plan: {ex.Message}");
            await SendErrorsAsync(statusCode: 500, cancellation: ct);
        }
    }
}

