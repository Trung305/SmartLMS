using SmartLMS.Core.Entities;

namespace SmartLMS.Web.Areas.Admin.Models;
public class AdminDashboardViewModel
{
    public int TotalCourses { get; set; }
    public int TotalUsers { get; set; }
    public int TotalEnrollments { get; set; }
    public int TotalActiveUsers { get; set; }
    public IList<Course> RecentCourses { get; set; } = new List<Course>();
    public IList<Enrollment> RecentEnrollments { get; set; } = new List<Enrollment>();
    public IList<MonthlyStatModel> MonthlyStats { get; set; } = new List<MonthlyStatModel>();
}

public class MonthlyStatModel
{
    public int Month { get; set; }
    public int Year { get; set; }
    public int Enrollments { get; set; }
    public decimal Revenue { get; set; }
}