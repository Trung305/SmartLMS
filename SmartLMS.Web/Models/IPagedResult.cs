namespace SmartLMS.Web.Models
{
    public interface IPagedResult
    {
        int CurrentPage { get; }
        int TotalPages { get; }
        int TotalItems { get; }
        bool HasPreviousPage { get; }
        bool HasNextPage { get; }
    }
}
