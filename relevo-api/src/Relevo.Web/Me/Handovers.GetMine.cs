using FastEndpoints;
using Relevo.Core.Interfaces;
using Relevo.Web.Setup;

namespace Relevo.Web.Me;

public class GetMyHandoversEndpoint(
    ISetupDataProvider _dataProvider,
    IUserContext _userContext)
  : Endpoint<GetMyHandoversRequest, GetMyHandoversResponse>
{
  public override void Configure()
  {
    Get("/me/handovers");
    AllowAnonymous(); // Let our custom middleware handle authentication
  }

  public override async Task HandleAsync(GetMyHandoversRequest req, CancellationToken ct)
  {
    // Get authenticated user from context
    var user = _userContext.CurrentUser;
    if (user == null)
    {
      await SendUnauthorizedAsync(ct);
      return;
    }

    var (handovers, total) = _dataProvider.GetMyHandovers(user.Id, req.Page <= 0 ? 1 : req.Page, req.PageSize <= 0 ? 25 : req.PageSize);
    Response = new GetMyHandoversResponse
    {
      Items = handovers.ToList(),
      Pagination = new PaginationInfo
      {
        TotalItems = total,
        CurrentPage = req.Page <= 0 ? 1 : req.Page,
        PageSize = req.PageSize <= 0 ? 25 : req.PageSize,
        TotalPages = (int)Math.Ceiling((double)total / (req.PageSize <= 0 ? 25 : req.PageSize))
      }
    };
    await SendAsync(Response, cancellation: ct);
  }
}

public class GetMyHandoversRequest
{
  public int Page { get; set; } = 1;
  public int PageSize { get; set; } = 25;
}

public class GetMyHandoversResponse
{
  public List<HandoverRecord> Items { get; set; } = [];
  public PaginationInfo Pagination { get; set; } = new();
}

public class PaginationInfo
{
  public int TotalItems { get; set; }
  public int TotalPages { get; set; }
  public int CurrentPage { get; set; }
  public int PageSize { get; set; }
}

public record HandoverRecord(
  string Id,
  string PatientId,
  string Status,
  HandoverIllnessSeverity IllnessSeverity,
  HandoverPatientSummary PatientSummary,
  List<HandoverActionItem> ActionItems,
  string SituationAwarenessDocId,
  HandoverSynthesis? Synthesis = null
);

public record HandoverIllnessSeverity(string Severity);

public record HandoverPatientSummary(string Content);

public record HandoverActionItem(string Id, string Description, bool IsCompleted);

public record HandoverSynthesis(string Content);
