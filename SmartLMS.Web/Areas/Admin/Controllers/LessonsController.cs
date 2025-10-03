using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartLMS.Core.Common;
using SmartLMS.Core.Entities;
using SmartLMS.Infrastructure.Data;
using SmartLMS.Web.Areas.Admin.Models;

namespace SmartLMS.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = Constants.ADMIN_ROLE + "," + Constants.INSTRUCTOR_ROLE)]
public class LessonsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<LessonsController> _logger;

    public LessonsController(ApplicationDbContext context, ILogger<LessonsController> logger)
    {
        _context = context;
        _logger = logger;
    }
    // GET: Admin/Lessons?courseId=xxx
    public async Task<IActionResult> Index(Guid courseId)
    {
        var course = await _context.Courses
            .Include(c => c.Category)
            .Include(c => c.Instructor)
            .FirstOrDefaultAsync(c => c.Id == courseId);

        if (course == null)
        {
            return NotFound();
        }

        var lessons = await _context.Lessons
            .Where(l => l.CourseId == courseId)
            .OrderBy(l => l.OrderIndex)
            .ToListAsync();

        var viewModel = new LessonIndexViewModel
        {
            Course = course,
            Lessons = lessons
        };

        return View(viewModel);
    }

    // GET: Admin/Lessons/Create?courseId=xxx
    [HttpGet]
    public async Task<IActionResult> Create(Guid courseId)
    {
        var course = await _context.Courses.FindAsync(courseId);
        if (course == null)
        {
            return NotFound();
        }

        // Get next order index
        var maxOrder = await _context.Lessons
            .Where(l => l.CourseId == courseId)
            .MaxAsync(l => (int?)l.OrderIndex) ?? 0;

        var viewModel = new LessonCreateEditViewModel
        {
            CourseId = courseId,
            CourseName = course.Title,
            OrderIndex = maxOrder + 1,
            IsPublished = true
        };

        return View(viewModel);
    }

    // POST: Admin/Lessons/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LessonCreateEditViewModel model, IFormFile? video)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var lesson = new Lesson
                {
                    CourseId = model.CourseId,
                    Title = model.Title,
                    Content = model.Content,
                    Duration = model.Duration,
                    OrderIndex = model.OrderIndex,
                    IsPreview = model.IsPreview,
                    IsPublished = model.IsPublished,
                    CreatedDate = DateTime.UtcNow
                };

                // Handle video upload
                if (video != null && video.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "videos");
                    Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(video.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await video.CopyToAsync(fileStream);
                    }

                    lesson.VideoUrl = "/uploads/videos/" + uniqueFileName;
                }

                _context.Lessons.Add(lesson);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Bài học đã được tạo thành công!";
                return RedirectToAction(nameof(Index), new { courseId = model.CourseId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating lesson");
                ModelState.AddModelError("", "Có lỗi xảy ra khi tạo bài học.");
            }
        }

        // Reload course name if validation fails
        var course = await _context.Courses.FindAsync(model.CourseId);
        if (course != null)
        {
            model.CourseName = course.Title;
        }

        return View(model);
    }

    // GET: Admin/Lessons/Edit/5
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var lesson = await _context.Lessons
            .Include(l => l.Course)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (lesson == null)
        {
            return NotFound();
        }

        var viewModel = new LessonCreateEditViewModel
        {
            Id = lesson.Id,
            CourseId = lesson.CourseId,
            CourseName = lesson.Course.Title,
            Title = lesson.Title,
            Content = lesson.Content,
            Duration = lesson.Duration,
            OrderIndex = lesson.OrderIndex,
            IsPreview = lesson.IsPreview,
            IsPublished = lesson.IsPublished,
            CurrentVideoUrl = lesson.VideoUrl
        };

        return View(viewModel);
    }

    // POST: Admin/Lessons/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(LessonCreateEditViewModel model, IFormFile? video)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var lesson = await _context.Lessons.FindAsync(model.Id);
                if (lesson == null)
                {
                    return NotFound();
                }

                lesson.Title = model.Title;
                lesson.Content = model.Content;
                lesson.Duration = model.Duration;
                lesson.OrderIndex = model.OrderIndex;
                lesson.IsPreview = model.IsPreview;
                lesson.IsPublished = model.IsPublished;

                // Handle new video upload
                if (video != null && video.Length > 0)
                {
                    // Delete old video if exists
                    if (!string.IsNullOrEmpty(lesson.VideoUrl))
                    {
                        var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", lesson.VideoUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "videos");
                    Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(video.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await video.CopyToAsync(fileStream);
                    }

                    lesson.VideoUrl = "/uploads/videos/" + uniqueFileName;
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Bài học đã được cập nhật thành công!";
                return RedirectToAction(nameof(Index), new { courseId = lesson.CourseId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating lesson");
                ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật bài học.");
            }
        }

        // Reload course name if validation fails
        var course = await _context.Courses.FindAsync(model.CourseId);
        if (course != null)
        {
            model.CourseName = course.Title;
        }

        return View(model);
    }

    // GET: Admin/Lessons/Details/5
    public async Task<IActionResult> Details(Guid id)
    {
        var lesson = await _context.Lessons
            .Include(l => l.Course)
                .ThenInclude(c => c.Category)
            .Include(l => l.Course)
                .ThenInclude(c => c.Instructor)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (lesson == null)
        {
            return NotFound();
        }

        return View(lesson);
    }

    // POST: Admin/Lessons/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var lesson = await _context.Lessons.FindAsync(id);
        if (lesson == null)
        {
            return NotFound();
        }

        var courseId = lesson.CourseId;

        // Check if lesson has progress records
        var hasProgress = await _context.LessonProgress.AnyAsync(lp => lp.LessonId == id);
        if (hasProgress)
        {
            TempData["ErrorMessage"] = "Không thể xóa bài học đã có học viên học.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // Delete video file if exists
        if (!string.IsNullOrEmpty(lesson.VideoUrl))
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", lesson.VideoUrl.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }

        _context.Lessons.Remove(lesson);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Bài học đã được xóa thành công!";
        return RedirectToAction(nameof(Index), new { courseId });
    }

    // POST: Admin/Lessons/TogglePublish
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TogglePublish(Guid id)
    {
        var lesson = await _context.Lessons.FindAsync(id);
        if (lesson == null)
        {
            return NotFound();
        }

        lesson.IsPublished = !lesson.IsPublished;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = lesson.IsPublished ?
            "Bài học đã được xuất bản." :
            "Bài học đã được ẩn.";

        return RedirectToAction(nameof(Details), new { id });
    }

    // POST: Admin/Lessons/Reorder
    [HttpPost]
    public async Task<IActionResult> Reorder(Guid courseId, List<Guid> lessonIds)
    {
        try
        {
            for (int i = 0; i < lessonIds.Count; i++)
            {
                var lesson = await _context.Lessons.FindAsync(lessonIds[i]);
                if (lesson != null && lesson.CourseId == courseId)
                {
                    lesson.OrderIndex = i + 1;
                }
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Đã cập nhật thứ tự bài học" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reordering lessons");
            return Json(new { success = false, message = "Có lỗi xảy ra" });
        }
    }
}