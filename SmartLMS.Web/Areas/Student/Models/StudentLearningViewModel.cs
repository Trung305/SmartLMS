using SmartLMS.Core.Entities;

namespace SmartLMS.Web.Areas.Student.Models;

public class StudentLearningViewModel
{
    public Enrollment Enrollment { get; set; } = new();
    public Lesson CurrentLesson { get; set; } = new();
    public LessonProgress LessonProgress { get; set; } = new();
    public LessonNavigationModel Navigation { get; set; } = new();
    public CourseStructureModel CourseStructure { get; set; } = new();
    public StudentNotes Notes { get; set; } = new();
}

public class LessonNavigationModel
{
    public Lesson? PreviousLesson { get; set; }
    public Lesson? NextLesson { get; set; }
    public bool CanGoNext { get; set; }
    public bool CanGoPrevious { get; set; }
    public string NextButtonText { get; set; } = "Bài tiếp theo";
    public bool IsLastLesson { get; set; }
    public bool IsFirstLesson { get; set; }
}

public class CourseStructureModel
{
    public IList<LessonStructureItem> Lessons { get; set; } = new List<LessonStructureItem>();
    public int CurrentLessonIndex { get; set; }
    public decimal CourseProgress { get; set; }
}

public class LessonStructureItem
{
    public Lesson Lesson { get; set; } = new();
    public bool IsCompleted { get; set; }
    public bool IsCurrentLesson { get; set; }
    public bool IsAccessible { get; set; }
    public decimal WatchPercentage { get; set; }
    public TimeSpan WatchTime { get; set; }
}

public class StudentNotes
{
    public string Content { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; }
    public int WordCount => Content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
}