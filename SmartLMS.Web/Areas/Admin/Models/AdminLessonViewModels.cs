using System.ComponentModel.DataAnnotations;
using SmartLMS.Core.Entities;

namespace SmartLMS.Web.Areas.Admin.Models;

/// <summary>
/// ViewModel cho trang danh sách bài học
/// </summary>
public class AdminLessonViewModels
{
    public Course Course { get; set; } = new();
    public IList<Lesson> Lessons { get; set; } = new List<Lesson>();

    // Computed properties
    public int TotalLessons => Lessons.Count;
    public int PublishedLessons => Lessons.Count(l => l.IsPublished);
    public int DraftLessons => Lessons.Count(l => !l.IsPublished);
    public int PreviewLessons => Lessons.Count(l => l.IsPreview);
    public TimeSpan TotalDuration => TimeSpan.FromSeconds(Lessons.Sum(l => l.Duration));
}

/// <summary>
/// ViewModel cho chi tiết bài học
/// </summary>
public class LessonDetailsViewModel
{
    public Lesson Lesson { get; set; } = new();
    public Course Course { get; set; } = new();
    public LessonStatistics Statistics { get; set; } = new();

    // Navigation
    public Lesson? PreviousLesson { get; set; }
    public Lesson? NextLesson { get; set; }
}

/// <summary>
/// Thống kê bài học
/// </summary>
public class LessonStatistics
{
    public int TotalViews { get; set; }
    public int CompletedCount { get; set; }
    public decimal CompletionRate { get; set; }
    public TimeSpan AverageWatchTime { get; set; }
    public int TotalComments { get; set; }
    public DateTime? LastAccessDate { get; set; }
}

/// <summary>
/// ViewModel cho reorder lessons
/// </summary>
public class LessonReorderViewModel
{
    public Guid CourseId { get; set; }
    public List<LessonOrderItem> Lessons { get; set; } = new();
}

public class LessonOrderItem
{
    public Guid LessonId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
}

/// <summary>
/// ViewModel cho bulk actions
/// </summary>
public class LessonBulkActionViewModel
{
    public Guid CourseId { get; set; }
    public List<Guid> SelectedLessonIds { get; set; } = new();
    public string Action { get; set; } = string.Empty; // publish, unpublish, delete
}

/// <summary>
/// ViewModel cho video upload result
/// </summary>
public class VideoUploadResult
{
    public bool Success { get; set; }
    public string? VideoUrl { get; set; }
    public int Duration { get; set; }
    public long FileSize { get; set; }
    public string? ErrorMessage { get; set; }

    public string FormattedFileSize => FormatFileSize(FileSize);

    private static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }
}

/// <summary>
/// ViewModel cho lesson progress tracking
/// </summary>
public class LessonProgressViewModel
{
    public Guid LessonId { get; set; }
    public string LessonTitle { get; set; } = string.Empty;
    public int TotalEnrollments { get; set; }
    public int CompletedCount { get; set; }
    public int InProgressCount { get; set; }
    public int NotStartedCount { get; set; }
    public decimal AverageProgress { get; set; }

    public decimal CompletionPercentage => TotalEnrollments > 0
        ? (decimal)CompletedCount / TotalEnrollments * 100
        : 0;
}

/// <summary>
/// ViewModel cho video processing status
/// </summary>
public class VideoProcessingStatus
{
    public Guid LessonId { get; set; }
    public string Status { get; set; } = string.Empty; // uploading, processing, ready, error
    public int ProgressPercentage { get; set; }
    public string? Message { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }

    public bool IsProcessing => Status == "processing" || Status == "uploading";
    public bool IsComplete => Status == "ready";
    public bool HasError => Status == "error";
    public TimeSpan? ProcessingTime => EndTime.HasValue
        ? EndTime.Value - StartTime
        : null;
}