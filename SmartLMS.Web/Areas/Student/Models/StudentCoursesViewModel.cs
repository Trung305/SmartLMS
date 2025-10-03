using SmartLMS.Core.Common;
using SmartLMS.Core.Entities;
using SmartLMS.Core.Enums;

namespace SmartLMS.Web.Areas.Student.Models;

public class StudentCoursesViewModel
{
    public PagedResult<Enrollment> Enrollments { get; set; } = new();
    public string? FilterStatus { get; set; }
    public string? SortBy { get; set; }
    public EnrollmentFilterOptions FilterOptions { get; set; } = new();
}

public class EnrollmentFilterOptions
{
    public Dictionary<string, string> StatusOptions { get; set; } = new()
    {
        { "", "Tất cả" },
        { "InProgress", "Đang học" },
        { "Completed", "Đã hoàn thành" },
        { "NotStarted", "Chưa bắt đầu" }
    };

    public Dictionary<string, string> SortOptions { get; set; } = new()
    {
        { "recent", "Gần đây nhất" },
        { "progress", "Tiến độ cao nhất" },
        { "alphabetical", "Tên A-Z" },
        { "completion", "Sắp hoàn thành" }
    };
}