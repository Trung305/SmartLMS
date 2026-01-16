using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartLMS.Core.Common;
using SmartLMS.Core.DTOs;
using SmartLMS.Core.Entities;
using SmartLMS.Core.Enums;
using SmartLMS.Core.Interfaces.Services;
using SmartLMS.Infrastructure.Services;
using SmartLMS.Web.Areas.Admin.Models;

namespace SmartLMS.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Instructor")]
    public class QuizzesController : Controller
    {
        private readonly IQuizzesService _quizzesService;
        private readonly ILogger<QuizzesController> _logger;

        public QuizzesController(
            IQuizzesService quizzesService,
            ILogger<QuizzesController> logger)
        {
            _quizzesService = quizzesService;
            _logger = logger;
        }
        #region Views

        // GET: Admin/Quizzes/Create
        public async Task<IActionResult> Create(Guid courseId, Guid? lessonId = null)
        {
            var result = await _quizzesService.GetCreateViewModelAsync(courseId, lessonId);

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.ErrorMessage;
                return RedirectToAction("Index", "Courses");
            }

            return PartialView("_CreateQuizModal", result.Data);
        }

        // GET: Admin/Quizzes/Edit/{id}
        public async Task<IActionResult> Edit(Guid id)
        {
            var result = await _quizzesService.GetEditViewModelAsync(id);

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.ErrorMessage;
                return RedirectToAction("Index", "Courses");
            }

            // Pass existing questions to JavaScript
            ViewBag.ExistingQuestions = result.Data.Questions;

            return PartialView("_EditQuizModal", result.Data);
        }

        // GET: Admin/Quizzes/Details/{id}
        public async Task<IActionResult> Details(Guid id)
        {
            var result = await _quizzesService.GetQuizByIdAsync(id);

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.ErrorMessage;
                return RedirectToAction("Index", "Courses");
            }

            return PartialView("_QuizDetailsModal", result.Data);
        }
        // GET: Admin/Quizzes/Details/{id}
        [HttpGet]
        public async Task<IActionResult> Upsert(Guid courseId, Guid? lessonId = null)
        {
            var result = await _quizzesService.GetCreateViewModelAsync(courseId, lessonId);
            if (lessonId.HasValue && lessonId.Value != Guid.Empty)
            {
                result = await _quizzesService.GetEditViewModelAsync(lessonId.Value);
                if (result == null)
                {
                    return NotFound();
                }
            }
            var viewModel = MapToViewModel(result.Data);
            return PartialView("Upsert", viewModel);
        }
        #endregion

        #region API Endpoints

        // POST: Admin/Quizzes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromBody] AdminCreateEditQuizViewModel model)
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
                    message = "Dữ liệu không hợp lệ: " + string.Join(", ", errors)
                });
            }

            Result<Guid> result;
            var dto = MapToDto(model);
            if (model.QuizId.HasValue)
            {
                result = await _quizzesService.UpdateQuizAsync(dto);
            }
            else
            {
                result = await _quizzesService.CreateQuizAsync(dto);
            }

            if (!result.IsSuccess)
            {
                return Json(new
                {
                    success = false,
                    message = result.ErrorMessage
                });
            }

            return Json(new
            {
                success = true,
                message = result.ErrorMessage,
                quizId = result.Data,
                courseId = model.CourseId
            });
        }

        // GET: Admin/Quizzes/GetQuizzesByCourse
        [HttpGet]
        public async Task<IActionResult> GetQuizzesByCourse(Guid courseId)
        {
            var result = await _quizzesService.GetQuizzesByCourseAsync(courseId);

            if (!result.IsSuccess)
            {
                return Json(new
                {
                    success = false,
                    message = result.ErrorMessage
                });
            }

            return Json(new
            {
                success = true,
                data = new { quizzes = result.Data }
            });
        }

        // DELETE: Admin/Quizzes/Delete/{id}
        [HttpDelete]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _quizzesService.DeleteQuizAsync(id);

            return Json(new
            {
                success = result.IsSuccess,
                message = result.ErrorMessage
            });
        }

        #endregion
        #region Private Helper Methods
        private AdminCreateEditQuizViewModel MapToViewModel(CreateEditQuizDto dto)
        {
            return new AdminCreateEditQuizViewModel
            {
                QuizId = dto.QuizId,
                CourseId = dto.CourseId,
                LessonId = dto.LessonId,

                Title = dto.Title,
                Description = dto.Description,

                TimeLimit = dto.TimeLimit,
                MaxAttempts = dto.MaxAttempts,
                PassingScore = dto.PassingScore,

                IsActive = dto.IsActive,
                IsTimedQuiz = dto.IsTimedQuiz,

                AvailableLessons = dto.AvailableLessons,
                Questions = dto.Questions
            };
        }

        private CreateEditQuizDto MapToDto(AdminCreateEditQuizViewModel model)
        {
            return new CreateEditQuizDto
            {
                QuizId = model.QuizId,
                CourseId = model.CourseId,
                LessonId = model.LessonId,

                Title = model.Title,
                Description = model.Description,

                TimeLimit = model.TimeLimit,
                MaxAttempts = model.MaxAttempts,
                PassingScore = model.PassingScore,

                IsActive = model.IsActive,
                IsTimedQuiz = model.IsTimedQuiz,

                AvailableLessons = model.AvailableLessons,
                Questions = model.Questions
            };
        }

        #endregion
        #region Helper Methods

        private IActionResult HandleServiceError(string message)
        {
            _logger.LogWarning("Service error: {Message}", message);
            TempData["Error"] = message;
            return RedirectToAction("Index", "Courses");
        }

        #endregion
    }
}
