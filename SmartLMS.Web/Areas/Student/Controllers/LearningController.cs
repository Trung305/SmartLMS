using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartLMS.Core.Common;
using SmartLMS.Core.Entities;
using SmartLMS.Infrastructure.Data;
using SmartLMS.Web.Areas.Student.Models;

namespace SmartLMS.Web.Areas.Student.Controllers;

[Area("Student")]
[Authorize(Roles = Constants.STUDENT_ROLE)]
public class LearningController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<LearningController> _logger;

    public LearningController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<LearningController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<IActionResult> Course(Guid id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        var enrollment = await _context.Enrollments
            .Where(e => e.StudentId == user.Id && e.CourseId == id)
            .Include(e => e.Course)
                .ThenInclude(c => c.Lessons.OrderBy(l => l.OrderIndex))
            .Include(e => e.LessonProgress)
            .FirstOrDefaultAsync();

        if (enrollment == null)
        {
            return NotFound("Bạn chưa đăng ký khóa học này.");
        }

        var currentLesson = enrollment.Course.Lessons
            .FirstOrDefault(l => !enrollment.LessonProgress.Any(lp => lp.LessonId == l.Id && lp.IsCompleted));

        currentLesson ??= enrollment.Course.Lessons.FirstOrDefault();

        if (currentLesson == null)
        {
            return NotFound("Khóa học chưa có bài học nào.");
        }

        return RedirectToAction(nameof(Lesson), new { courseId = id, lessonId = currentLesson.Id });
    }

    public async Task<IActionResult> Lesson(Guid courseId, Guid lessonId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        var enrollment = await _context.Enrollments
            .Where(e => e.StudentId == user.Id && e.CourseId == courseId)
            .Include(e => e.Course)
                .ThenInclude(c => c.Lessons.OrderBy(l => l.OrderIndex))
            .Include(e => e.LessonProgress)
            .FirstOrDefaultAsync();

        if (enrollment == null)
        {
            return NotFound("Bạn chưa đăng ký khóa học này.");
        }

        var lesson = enrollment.Course.Lessons.FirstOrDefault(l => l.Id == lessonId);
        if (lesson == null)
        {
            return NotFound("Bài học không tồn tại.");
        }

        var lessonProgress = await _context.LessonProgress
            .FirstOrDefaultAsync(lp => lp.EnrollmentId == enrollment.Id && lp.LessonId == lessonId);

        if (lessonProgress == null)
        {
            lessonProgress = new LessonProgress
            {
                EnrollmentId = enrollment.Id,
                LessonId = lessonId,
                FirstWatchedDate = DateTime.UtcNow
            };
            _context.LessonProgress.Add(lessonProgress);
            await _context.SaveChangesAsync();
        }
        else
        {
            lessonProgress.LastAccessDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        var viewModel = new LessonViewModel
        {
            Enrollment = enrollment,
            CurrentLesson = lesson,
            LessonProgress = lessonProgress,
            NextLesson = enrollment.Course.Lessons
                .Where(l => l.OrderIndex > lesson.OrderIndex)
                .OrderBy(l => l.OrderIndex)
                .FirstOrDefault(),
            PreviousLesson = enrollment.Course.Lessons
                .Where(l => l.OrderIndex < lesson.OrderIndex)
                .OrderByDescending(l => l.OrderIndex)
                .FirstOrDefault()
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateProgress(Guid lessonId, int watchedDuration)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Json(new { success = false, message = "Unauthorized" });
        }

        var lessonProgress = await _context.LessonProgress
            .Include(lp => lp.Enrollment)
            .Include(lp => lp.Lesson)
            .FirstOrDefaultAsync(lp => lp.LessonId == lessonId && lp.Enrollment.StudentId == user.Id);

        if (lessonProgress == null)
        {
            return Json(new { success = false, message = "Lesson progress not found" });
        }

        lessonProgress.WatchedDuration = Math.Max(lessonProgress.WatchedDuration, watchedDuration);
        lessonProgress.LastAccessDate = DateTime.UtcNow;

        // Mark as completed if watched 90% of the video
        var completionThreshold = lessonProgress.Lesson.Duration * 0.9;
        if (!lessonProgress.IsCompleted && watchedDuration >= completionThreshold)
        {
            lessonProgress.IsCompleted = true;
            lessonProgress.CompletedDate = DateTime.UtcNow;

            // Update enrollment progress
            await UpdateEnrollmentProgressAsync(lessonProgress.EnrollmentId);
        }

        await _context.SaveChangesAsync();

        return Json(new
        {
            success = true,
            isCompleted = lessonProgress.IsCompleted,
            watchPercentage = lessonProgress.WatchPercentage
        });
    }

    private async Task UpdateEnrollmentProgressAsync(Guid enrollmentId)
    {
        var enrollment = await _context.Enrollments
            .Include(e => e.Course)
                .ThenInclude(c => c.Lessons)
            .Include(e => e.LessonProgress)
            .FirstOrDefaultAsync(e => e.Id == enrollmentId);

        if (enrollment == null) return;

        var totalLessons = enrollment.Course.Lessons.Count;
        var completedLessons = enrollment.LessonProgress.Count(lp => lp.IsCompleted);

        enrollment.Progress = totalLessons > 0 ? (decimal)completedLessons / totalLessons * 100 : 0;

        if (enrollment.Progress >= 100 && !enrollment.CompletionDate.HasValue)
        {
            enrollment.CompletionDate = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }
}