namespace SmartLMS.Web.Areas.Admin.Models;

public class AdminReportsViewModel
{
    public SystemOverviewReport SystemOverview { get; set; } = new();
    public IList<MonthlyReportData> MonthlyReports { get; set; } = new List<MonthlyReportData>();
    public IList<PopularCourseReport> PopularCourses { get; set; } = new List<PopularCourseReport>();
    public IList<InstructorPerformanceReport> InstructorPerformance { get; set; } = new List<InstructorPerformanceReport>();
    public DateTime ReportStartDate { get; set; } = DateTime.UtcNow.AddMonths(-6);
    public DateTime ReportEndDate { get; set; } = DateTime.UtcNow;
}

public class SystemOverviewReport
{
    public int TotalUsers { get; set; }
    public int TotalStudents { get; set; }
    public int TotalInstructors { get; set; }
    public int TotalCourses { get; set; }
    public int PublishedCourses { get; set; }
    public int TotalEnrollments { get; set; }
    public int ActiveEnrollments { get; set; }
    public int CompletedEnrollments { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageCompletionRate { get; set; }
    public int NewUsersThisMonth { get; set; }
    public int NewCoursesThisMonth { get; set; }
    public int NewEnrollmentsThisMonth { get; set; }
}

public class MonthlyReportData
{
    public int Month { get; set; }
    public int Year { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public int NewUsers { get; set; }
    public int NewCourses { get; set; }
    public int NewEnrollments { get; set; }
    public int CompletedCourses { get; set; }
    public decimal Revenue { get; set; }
}

public class PopularCourseReport
{
    public Guid CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string InstructorName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int EnrollmentCount { get; set; }
    public int CompletionCount { get; set; }
    public decimal CompletionRate { get; set; }
    public decimal Revenue { get; set; }
    public decimal AverageRating { get; set; }
}

public class InstructorPerformanceReport
{
    public Guid InstructorId { get; set; }
    public string InstructorName { get; set; } = string.Empty;
    public int TotalCourses { get; set; }
    public int PublishedCourses { get; set; }
    public int TotalEnrollments { get; set; }
    public int CompletedEnrollments { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageRating { get; set; }
    public decimal AverageCompletionRate { get; set; }
}