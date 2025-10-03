using SmartLMS.Core.Entities;

namespace SmartLMS.Web.Models.ViewModels;

public class HomeViewModel
{
    public IList<Course> FeaturedCourses { get; set; } = new List<Course>();
    public IList<Category> Categories { get; set; } = new List<Category>();
    public HomeStatsViewModel Stats { get; set; } = new();
}

public class HomeStatsViewModel
{
    public int TotalCourses { get; set; }
    public int TotalStudents { get; set; }
    public int TotalInstructors { get; set; }
}