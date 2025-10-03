using SmartLMS.Core.Entities;

namespace SmartLMS.Web.Areas.Student.Models;

public class StudentDashboardViewModel
{
    public ApplicationUser Student { get; set; } = new();
    public int TotalEnrollments { get; set; }
    public int CompletedCourses { get; set; }
    public int InProgressCourses { get; set; }
    public int TotalLearningTime { get; set; }
    public decimal OverallProgress { get; set; }
    public IList<Enrollment> RecentEnrollments { get; set; } = new List<Enrollment>();
    public IList<LessonProgress> RecentActivity { get; set; } = new List<LessonProgress>();
}

public class LessonViewModel
{
    public Enrollment Enrollment { get; set; } = new();
    public Lesson CurrentLesson { get; set; } = new();
    public LessonProgress LessonProgress { get; set; } = new();
    public Lesson? NextLesson { get; set; }
    public Lesson? PreviousLesson { get; set; }
}