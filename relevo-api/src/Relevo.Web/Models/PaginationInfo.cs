namespace Relevo.Web.Models;

public class PaginationInfo
{
    public int TotalCount { get; set; }
    public int TotalItems { get; set; }
    public int Page { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }

    // Computed properties for backward compatibility
    public int TotalPagesComputed => PageSize > 0 ? (int)Math.Ceiling((double)(TotalCount > 0 ? TotalCount : TotalItems) / PageSize) : 0;
    public int PageOrCurrentPage => Page > 0 ? Page : CurrentPage;
}
