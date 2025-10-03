using SmartLMS.Core.Common;
using SmartLMS.Core.Entities;

namespace SmartLMS.Web.Models.ViewModels;

public class SearchResultViewModel
{
    public string SearchQuery { get; set; } = string.Empty;
    public PagedResult<Course> Courses { get; set; } = new();
    public IList<Category> Categories { get; set; } = new List<Category>();
    public SearchFilters Filters { get; set; } = new();
    public int TotalResults { get; set; }
}

public class SearchFilters
{
    public int? CategoryId { get; set; }
    public string? Level { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? IsFree { get; set; }
    public string? SortBy { get; set; }
    public string? Duration { get; set; }
    public bool? HasCertificate { get; set; }
}

public class NotificationViewModel
{
    public string Type { get; set; } = "info"; // success, error, warning, info
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool AutoHide { get; set; } = true;
    public int HideAfter { get; set; } = 5000; // milliseconds
    public string? ActionUrl { get; set; }
    public string? ActionText { get; set; }
}

public class BreadcrumbViewModel
{
    public IList<BreadcrumbItem> Items { get; set; } = new List<BreadcrumbItem>();
}

public class BreadcrumbItem
{
    public string Text { get; set; } = string.Empty;
    public string? Url { get; set; }
    public bool IsActive { get; set; }
    public string? Icon { get; set; }
}