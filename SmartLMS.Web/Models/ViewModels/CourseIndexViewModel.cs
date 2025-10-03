using SmartLMS.Core.Common;
using SmartLMS.Core.Entities;

namespace SmartLMS.Web.Models.ViewModels;

public class CourseIndexViewModel
{
    public PagedResult<Course> Courses { get; set; } = new();
    public IList<Category> Categories { get; set; } = new List<Category>();
    public int? SelectedCategoryId { get; set; }
    public string? SelectedLevel { get; set; }
    public string? SearchQuery { get; set; }
}