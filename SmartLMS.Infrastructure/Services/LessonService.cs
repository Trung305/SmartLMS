using SmartLMS.Core.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartLMS.Core.Common;
using SmartLMS.Core.DTOs;
using SmartLMS.Core.Entities;
using SmartLMS.Core.Enums;
using SmartLMS.Core.Interfaces.Services;
using SmartLMS.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace SmartLMS.Infrastructure.Services
{
    public class LessonService : ILessonService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LessonService> _logger;
        public LessonService(ApplicationDbContext context, ILogger<LessonService> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<Result<LessonIndexDto>> GetLessonsByCourseAsync(Guid courseId)
        {
            try
            {
                var course = await _context.Courses
                    .Include(c => c.Category)
                    .Include(c => c.Instructor)
                    .FirstOrDefaultAsync(c => c.Id == courseId);

                if (course == null)
                {
                    return Result<LessonIndexDto>.Fail("Không tìm thấy khóa học.");
                }

                var lessons = await _context.Lessons
                    .Where(l => l.CourseId == courseId)
                    .OrderBy(l => l.OrderIndex)
                    .ToListAsync();
                var quizzes = await _context.Quizzes
                    .Where(q => q.CourseId == courseId)
                    .Include(q => q.Questions)
                    .OrderByDescending(q => q.CreatedDate)
                    .Select(q => new QuizDto
                    {
                        Id = q.Id,
                        CourseId = q.CourseId,
                        LessonId = q.LessonId,
                        Title = q.Title,
                        Description = q.Description,
                        TimeLimit = q.TimeLimit,
                        MaxAttempts = q.MaxAttempts,
                        PassingScore = q.PassingScore,
                        IsActive = q.IsActive,
                        IsTimedQuiz = q.IsTimedQuiz,
                        CreatedDate = q.CreatedDate,
                        TotalQuestions = q.Questions.Count,
                        TotalPoints = q.Questions.Sum(qu => qu.Points)
                    })
                    .ToListAsync();
                var dto = new LessonIndexDto
                {
                    Course = course,
                    Lessons = lessons,
                    Quizzes = quizzes
                };

                return Result<LessonIndexDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting lessons for course {CourseId}", courseId);
                return Result<LessonIndexDto>.Fail("Có lỗi xảy ra khi lấy danh sách bài học.");
            }
        }

        public async Task<Result<LessonCreateEditDto>> GetCreateDtoAsync(Guid courseId)
        {
            try
            {
                var course = await _context.Courses.FindAsync(courseId);
                if (course == null)
                {
                    return Result<LessonCreateEditDto>.Fail("Không tìm thấy khóa học.");
                }

                var maxOrder = await _context.Lessons
                    .Where(l => l.CourseId == courseId)
                    .MaxAsync(l => (int?)l.OrderIndex) ?? 0;

                var dto = new LessonCreateEditDto
                {
                    CourseId = courseId,
                    CourseName = course.Title,
                    OrderIndex = maxOrder + 1,
                    IsPublished = true,
                    IsPreview = false
                };

                return Result<LessonCreateEditDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting create DTO for course {CourseId}", courseId);
                return Result<LessonCreateEditDto>.Fail("Có lỗi xảy ra.");
            }
        }

        public async Task<Result<Guid>> CreateLessonAsync(LessonCreateEditDto dto, IFormFile? video)
        {
            try
            {
                var courseExists = await _context.Courses.AnyAsync(c => c.Id == dto.CourseId);
                if (!courseExists)
                {
                    return Result<Guid>.Fail("Không tìm thấy khóa học.");
                }

                var lesson = new Lesson
                {
                    Id = Guid.NewGuid(),
                    CourseId = dto.CourseId,
                    Title = dto.Title,
                    Content = dto.Content,
                    Duration = dto.Duration,
                    OrderIndex = dto.OrderIndex,
                    IsPreview = dto.IsPreview,
                    IsPublished = dto.IsPublished,
                    CreatedDate = DateTime.UtcNow
                };

                if (video != null && video.Length > 0)
                {
                    var videoUrl = await SaveVideoFileAsync(video);
                    lesson.VideoUrl = videoUrl;
                }

                _context.Lessons.Add(lesson);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created lesson {LessonId} for course {CourseId}", lesson.Id, dto.CourseId);

                return Result<Guid>.Success(lesson.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating lesson for course {CourseId}", dto.CourseId);
                return Result<Guid>.Fail("Có lỗi xảy ra khi tạo bài học.");
            }
        }

        public async Task<Result<LessonCreateEditDto>> GetEditDtoAsync(Guid lessonId)
        {
            try
            {
                var lesson = await _context.Lessons
                    .Include(l => l.Course)
                    .FirstOrDefaultAsync(l => l.Id == lessonId);

                if (lesson == null)
                {
                    return Result<LessonCreateEditDto>.Fail("Không tìm thấy bài học.");
                }

                var dto = new LessonCreateEditDto
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

                return Result<LessonCreateEditDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting edit DTO for lesson {LessonId}", lessonId);
                return Result<LessonCreateEditDto>.Fail("Có lỗi xảy ra.");
            }
        }

        public async Task<Result> UpdateLessonAsync(LessonCreateEditDto dto, IFormFile? video)
        {
            try
            {
                var lesson = await _context.Lessons.FindAsync(dto.Id);
                if (lesson == null)
                {
                    return Result.Fail("Không tìm thấy bài học.");
                }

                lesson.Title = dto.Title;
                lesson.Content = dto.Content;
                lesson.Duration = dto.Duration;
                lesson.OrderIndex = dto.OrderIndex;
                lesson.IsPreview = dto.IsPreview;
                lesson.IsPublished = dto.IsPublished;

                if (video != null && video.Length > 0)
                {

                    if (!string.IsNullOrEmpty(lesson.VideoUrl))
                    {
                        DeleteVideoFile(lesson.VideoUrl);
                    }

                    lesson.VideoUrl = await SaveVideoFileAsync(video);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated lesson {LessonId}", dto.Id);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating lesson {LessonId}", dto.Id);
                return Result.Fail("Có lỗi xảy ra khi cập nhật bài học.");
            }
        }

        public async Task<Result<Lesson>> GetLessonDetailsAsync(Guid lessonId)
        {
            try
            {
                var lesson = await _context.Lessons
                    .Include(l => l.Course)
                        .ThenInclude(c => c.Category)
                    .Include(l => l.Course)
                        .ThenInclude(c => c.Instructor)
                    .FirstOrDefaultAsync(l => l.Id == lessonId);

                if (lesson == null)
                {
                    return Result<Lesson>.Fail("Không tìm thấy bài học.");
                }

                return Result<Lesson>.Success(lesson);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting lesson details {LessonId}", lessonId);
                return Result<Lesson>.Fail("Có lỗi xảy ra.");
            }
        }

        public async Task<Result> DeleteLessonAsync(Guid lessonId)
        {
            try
            {
                var lesson = await _context.Lessons.FindAsync(lessonId);
                if (lesson == null)
                {
                    return Result.Fail("Không tìm thấy bài học.");
                }

                var hasProgress = await _context.LessonProgress
                    .AnyAsync(lp => lp.LessonId == lessonId);

                if (hasProgress)
                {
                    return Result.Fail("Không thể xóa bài học đã có học viên học.");
                }

                if (!string.IsNullOrEmpty(lesson.VideoUrl))
                {
                    DeleteVideoFile(lesson.VideoUrl);
                }

                _context.Lessons.Remove(lesson);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted lesson {LessonId}", lessonId);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting lesson {LessonId}", lessonId);
                return Result.Fail("Có lỗi xảy ra khi xóa bài học.");
            }
        }

        public async Task<Result> TogglePublishAsync(Guid lessonId)
        {
            try
            {
                var lesson = await _context.Lessons.FindAsync(lessonId);
                if (lesson == null)
                {
                    return Result.Fail("Không tìm thấy bài học.");
                }

                lesson.IsPublished = !lesson.IsPublished;
                await _context.SaveChangesAsync();

                var message = lesson.IsPublished
                    ? "Bài học đã được xuất bản."
                    : "Bài học đã được ẩn.";

                _logger.LogInformation("Toggled publish status for lesson {LessonId} to {IsPublished}",
                    lessonId, lesson.IsPublished);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling publish for lesson {LessonId}", lessonId);
                return Result.Fail("Có lỗi xảy ra.");
            }
        }

        public async Task<Result> ReorderLessonsAsync(Guid courseId, List<Guid> lessonIds)
        {
            try
            {
                var lessons = await _context.Lessons
                    .Where(l => l.CourseId == courseId && lessonIds.Contains(l.Id))
                    .ToListAsync();

                if (lessons.Count != lessonIds.Count)
                {
                    return Result.Fail("Một số bài học không thuộc khóa học này.");
                }

                for (int i = 0; i < lessonIds.Count; i++)
                {
                    var lesson = lessons.FirstOrDefault(l => l.Id == lessonIds[i]);
                    if (lesson != null)
                    {
                        lesson.OrderIndex = i + 1;
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Reordered {Count} lessons for course {CourseId}",
                    lessonIds.Count, courseId);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reordering lessons for course {CourseId}", courseId);
                return Result.Fail("Có lỗi xảy ra khi sắp xếp lại bài học.");
            }
        }

        #region Private Helper Methods

        private async Task<string> SaveVideoFileAsync(IFormFile video)
        {
            var uploadsFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads",
                "videos"
            );

            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(video.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await video.CopyToAsync(fileStream);
            }

            _logger.LogInformation("Saved video file: {FileName}", uniqueFileName);

            return $"/uploads/videos/{uniqueFileName}";
        }

        private void DeleteVideoFile(string videoUrl)
        {
            try
            {
                var filePath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    videoUrl.TrimStart('/')
                );

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger.LogInformation("Deleted video file: {FilePath}", filePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete video file: {VideoUrl}", videoUrl);
            }
        }

        #endregion
    }
}
