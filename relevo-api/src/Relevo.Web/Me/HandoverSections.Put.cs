using FastEndpoints;
using Relevo.Core.Interfaces;

namespace Relevo.Web.Me;

public class UpdateHandoverSectionEndpoint(
    ISetupService _setupService,
    IUserContext _userContext)
  : Endpoint<UpdateHandoverSectionRequest, UpdateHandoverSectionResponse>
{
  public override void Configure()
  {
    Put("/me/handovers/{handoverId}/sections/{sectionId}");
    AllowAnonymous(); // Let our custom middleware handle authentication
  }

  public override async Task HandleAsync(UpdateHandoverSectionRequest req, CancellationToken ct)
  {
    // Get authenticated user from context
    var user = _userContext.CurrentUser;
    if (user == null)
    {
      await SendUnauthorizedAsync(ct);
      return;
    }

    // Update the section
    var success = await _setupService.UpdateHandoverSectionAsync(
        req.HandoverId,
        req.SectionId,
        req.Content,
        req.Status,
        user.Id
    );

    if (!success)
    {
      await SendNotFoundAsync(ct);
      return;
    }

    Response = new UpdateHandoverSectionResponse
    {
        Success = true,
        Message = "Section updated successfully"
    };

    await SendAsync(Response, cancellation: ct);
  }
}

public class UpdateHandoverSectionRequest
{
    public required string HandoverId { get; set; }
    public required string SectionId { get; set; }
    public required string Content { get; set; }
    public required string Status { get; set; }
}

public class UpdateHandoverSectionResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
