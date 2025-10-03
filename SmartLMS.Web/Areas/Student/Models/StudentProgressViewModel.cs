using SmartLMS.Core.Entities;

namespace SmartLMS.Web.Areas.Student.Models;

public class StudentProgressViewModel
{
    public ApplicationUser Student { get; set; } = new();
    public Enrollment Enrollment { get; set; } = new();
    public IList<LessonProgressDetail> LessonsProgress { get; set; } = new List<LessonProgressDetail>();
    public CourseProgressSummary ProgressSummary { get; set; } = new();
    public IList<QuizAttemptSummary> QuizAttempts { get; set; } = new List<QuizAttemptSummary>();
}

public class LessonProgressDetail
{
    public Lesson Lesson { get; set; } = new();
    public LessonProgress? Progress { get; set; }
    public bool IsAccessible { get; set; } = true;
    public string StatusText { get; set; } = string.Empty;
    public string StatusClass { get; set; } = string.Empty;
}

public class CourseProgressSummary
{
    public int TotalLessons { get; set; }
    public int CompletedLessons { get; set; }
    public int InProgressLessons { get; set; }
    public int NotStartedLessons { get; set; }
    public decimal OverallProgress { get; set; }
    public TimeSpan TotalWatchTime { get; set; }
    public TimeSpan EstimatedTimeRemaining { get; set; }
    public DateTime? EstimatedCompletionDate { get; set; }
    public int StreakDays { get; set; }
    public DateTime? LastActivityDate { get; set; }
}

public class QuizAttemptSummary
{
    public Quiz Quiz { get; set; } = new();
    public QuizAttempt? BestAttempt { get; set; }
    public int TotalAttempts { get; set; }
    public bool IsPassed { get; set; }
    public int RemainingAttempts { get; set; }
    public bool CanRetake { get; set; }
}