using SmartLMS.Core.Common;
using SmartLMS.Core.Entities;

namespace SmartLMS.Web.Areas.Admin.Models;

public class AdminCoursesViewModel
{
    public PagedResult<Course> Courses { get; set; } = new();
    public string? SearchQuery { get; set; }
    public string? SelectedCategory { get; set; }
    public string? SelectedStatus { get; set; }
    public IList<Category> Categories { get; set; } = new List<Category>();
}

public class AdminCourseDetailsViewModel
{
    public Course Course { get; set; } = new();
    public IList<Enrollment> RecentEnrollments { get; set; } = new List<Enrollment>();
    public CourseStatistics Statistics { get; set; } = new();
}

public class CourseStatistics
{
    public int TotalEnrollments { get; set; }
    public int ActiveEnrollments { get; set; }
    public int CompletedEnrollments { get; set; }
    public decimal CompletionRate { get; set; }
    public decimal AverageProgress { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalLessons { get; set; }
    public int TotalQuizzes { get; set; }
    public DateTime? LastEnrollmentDate { get; set; }
}