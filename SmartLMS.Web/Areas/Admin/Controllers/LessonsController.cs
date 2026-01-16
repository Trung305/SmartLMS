using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartLMS.Core.Common;
using SmartLMS.Core.DTOs;
using SmartLMS.Core.Entities;
using SmartLMS.Core.Interfaces.Services;
using SmartLMS.Infrastructure.Data;
using SmartLMS.Web.Areas.Admin.Models;

namespace SmartLMS.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = Constants.ADMIN_ROLE + "," + Constants.INSTRUCTOR_ROLE)]
public class LessonsController : Controller
{
    private readonly ILessonService _lessonService;
    private readonly IQuizzesService _quizzesService;
    private readonly ILogger<LessonsController> _logger;

    public LessonsController(ILessonService lessonService, IQuizzesService quizzesService, ILogger<LessonsController> logger)
    {
        _lessonService = lessonService;
        _quizzesService = quizzesService;
        _logger = logger;
    }

    // GET: Admin/Lessons?courseId=xxx
    public async Task<IActionResult> Index(Guid courseId)
    {
        var result = await _lessonService.GetLessonsByCourseAsync(courseId);

        if (!result.IsSuccess)
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
            return NotFound();
        }

        var viewModel = new LessonIndexViewModel
        {
            Course = result.Data.Course,
            Lessons = result.Data.Lessons
        };

        return PartialView("Index", viewModel);
    }
    [HttpGet]
    public async Task<IActionResult> GetLessonsByCourse([FromQuery] Guid courseId)
    {
        if (courseId == Guid.Empty)
        {
            return BadRequest(new { success = false, message = "courseId không hợp lệ" });
        }

        var result = await _lessonService.GetLessonsByCourseAsync(courseId);
        var quizzesTask = await _quizzesService.GetQuizzesByCourseAsync(courseId);
        if (!result.IsSuccess)
        {
            return NotFound(new { success = false, message = result.ErrorMessage });
        }

        return Ok(new
        {
            success = true,
            data = new
            {
                course = new
                {
                    id = result.Data.Course.Id,
                    title = result.Data.Course.Title,
                    thumbnail = result.Data.Course.Thumbnail,
                    instructor = new
                    {
                        fullName = result.Data.Course.Instructor?.FullName
                    },
                    category = new
                    {
                        name = result.Data.Course.Category?.Name
                    }
                },
                lessons = result.Data.Lessons.Select(l => new
                {
                    id = l.Id,
                    title = l.Title,
                    content = l.Content,
                    videoUrl = l.VideoUrl,
                    duration = l.Duration,
                    orderIndex = l.OrderIndex,
                    isPreview = l.IsPreview,
                    isPublished = l.IsPublished,
                    createdDate = l.CreatedDate
                }).ToList(),
                quizzes = quizzesTask.Data
            }
        });
    }
    // GET: Admin/Lessons/Create?courseId=xxx
    [HttpGet]
    public async Task<IActionResult> Create(Guid courseId)
    {
        var result = await _lessonService.GetCreateDtoAsync(courseId);

        if (!result.IsSuccess)
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
            return NotFound();
        }

        var viewModel = MapToViewModel(result.Data);

        return PartialView("Create", viewModel);
    }

    // POST: Admin/Lessons/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LessonCreateEditViewModel model, IFormFile? video)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return Json(new
                {
                    success = false,
                    message = "Vui lòng kiểm tra lại thông tin!",
                    errors = errors
                });
            }

            var dto = MapToDto(model);
            var result = await _lessonService.CreateLessonAsync(dto, video);

            if (!result.IsSuccess)
            {
                return Json(new
                {
                    success = false,
                    message = result.ErrorMessage
                });
            }

            _logger.LogInformation("Created lesson {LessonId} for course {CourseId}", result.Data, model.CourseId);

            return Json(new
            {
                success = true,
                message = "Bài học đã được tạo thành công!",
                courseId = model.CourseId,
                redirectUrl = Url.Action("Index", new { courseId = model.CourseId })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating lesson for course {CourseId}", model.CourseId);

            return Json(new
            {
                success = false,
                message = "Có lỗi xảy ra, vui lòng liên hệ quản trị!"
            });
        }
    }

    // GET: Admin/Lessons/Edit/5
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var result = await _lessonService.GetEditDtoAsync(id);

        if (!result.IsSuccess)
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
            return NotFound();
        }

        var viewModel = MapToViewModel(result.Data);

        return PartialView("Edit", viewModel);
    }

    // POST: Admin/Lessons/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(LessonCreateEditViewModel model, IFormFile? video)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return Json(new
                {
                    success = false,
                    message = "Vui lòng kiểm tra lại thông tin!",
                    errors = errors
                });
            }

            var dto = MapToDto(model);
            var result = await _lessonService.UpdateLessonAsync(dto, video);

            if (!result.IsSuccess)
            {
                return Json(new
                {
                    success = false,
                    message = result.ErrorMessage
                });
            }

            _logger.LogInformation("Updated lesson {LessonId}", model.Id);

            return Json(new
            {
                success = true,
                message = "Bài học đã được cập nhật thành công!",
                courseId = model.CourseId,
                redirectUrl = Url.Action("Index", new { courseId = model.CourseId })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating lesson {LessonId}", model.Id);

            return Json(new
            {
                success = false,
                message = "Có lỗi xảy ra, vui lòng liên hệ quản trị!"
            });
        }
    }

    // GET: Admin/Lessons/Details/5
    public async Task<IActionResult> Details(Guid id)
    {
        var result = await _lessonService.GetLessonDetailsAsync(id);

        if (!result.IsSuccess)
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
            return NotFound();
        }

        return PartialView("Details", result.Data);
    }

    // DELETE: Admin/Lessons/Delete/5
    [HttpDelete]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _lessonService.DeleteLessonAsync(id);

        return Json(new
        {
            success = result.IsSuccess,
            message = result.IsSuccess ? "Xóa thành công" : result.ErrorMessage
        });
    }

    // POST: Admin/Lessons/TogglePublish
    [HttpPost]
    public async Task<IActionResult> TogglePublish(Guid id)
    {
        var result = await _lessonService.TogglePublishAsync(id);

        return Json(new
        {
            success = result.IsSuccess,
            message = result.IsSuccess ? "Thành công" : result.ErrorMessage
        });
    }

    // POST: Admin/Lessons/Reorder
    [HttpPost]
    public async Task<IActionResult> Reorder([FromBody] TogglePublishCommand cmd)
    {
        if (cmd.courseId == null || !cmd.lessonIds.Any())
        {
            return Json(new { success = false, message = "Danh sách bài học không hợp lệ" });
        }

        var result = await _lessonService.ReorderLessonsAsync(cmd.courseId, cmd.lessonIds);

        return Json(new
        {
            success = result.IsSuccess,
            message = result.ErrorMessage
        });
    }

    #region Private Helper Methods
    private LessonCreateEditViewModel MapToViewModel(LessonCreateEditDto dto)
    {
        return new LessonCreateEditViewModel
        {
            Id = dto.Id,
            CourseId = dto.CourseId,
            CourseName = dto.CourseName,
            Title = dto.Title,
            Content = dto.Content,
            Duration = dto.Duration,
            OrderIndex = dto.OrderIndex,
            IsPreview = dto.IsPreview,
            IsPublished = dto.IsPublished,
            CurrentVideoUrl = dto.CurrentVideoUrl
        };
    }

    private LessonCreateEditDto MapToDto(LessonCreateEditViewModel model)
    {
        return new LessonCreateEditDto
        {
            Id = model.Id,
            CourseId = model.CourseId,
            Title = model.Title,
            Content = model.Content,
            Duration = model.Duration,
            OrderIndex = model.OrderIndex,
            IsPreview = model.IsPreview,
            IsPublished = model.IsPublished
        };
    }

    #endregion

    public class TogglePublishCommand
    {
        public Guid courseId { get; set; }
        public List<Guid> lessonIds { get; set; }
    }
}