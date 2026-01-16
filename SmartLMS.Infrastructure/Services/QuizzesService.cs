using Microsoft.Extensions.Logging;
using SmartLMS.Core.Common;
using SmartLMS.Core.DTOs;
using SmartLMS.Core.Entities;
using SmartLMS.Core.Enums;
using SmartLMS.Core.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using SmartLMS.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLMS.Infrastructure.Services
{
    public class QuizzesService : IQuizzesService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<QuizzesService> _logger;

        public QuizzesService(
            ApplicationDbContext context,
            ILogger<QuizzesService> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Get Quiz Data

        public async Task<Result<QuizDetailDto>> GetQuizByIdAsync(Guid quizId)
        {
            try
            {
                var quiz = await _context.Quizzes
                    .Include(q => q.Course)
                    .Include(q => q.Lesson)
                    .Include(q => q.Questions)
                        .ThenInclude(q => q.Answers)
                    .Include(q => q.QuizAttempts)
                    .FirstOrDefaultAsync(q => q.Id == quizId);

                if (quiz == null)
                {
                    return Result<QuizDetailDto>.Fail("Không tìm thấy bài kiểm tra");
                }

                var dto = MapToQuizDetailDto(quiz);
                return Result<QuizDetailDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting quiz {QuizId}", quizId);
                return Result<QuizDetailDto>.Fail("Có lỗi xảy ra khi tải dữ liệu");
            }
        }

        public async Task<Result<List<QuizDto>>> GetQuizzesByCourseAsync(Guid courseId)
        {
            try
            {
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

                return Result<List<QuizDto>>.Success(quizzes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting quizzes for course {CourseId}", courseId);
                return Result<List<QuizDto>>.Fail("Có lỗi xảy ra khi tải danh sách bài kiểm tra");
            }
        }

        public async Task<Result<CreateEditQuizDto>> GetCreateViewModelAsync(Guid courseId, Guid? lessonId = null)
        {
            try
            {
                var course = await _context.Courses.FindAsync(courseId);
                if (course == null)
                {
                    return Result<CreateEditQuizDto>.Fail("Khóa học không tồn tại");
                }

                var lessons = await GetLessonsForCourseAsync(courseId);

                var model = new CreateEditQuizDto
                {
                    CourseId = courseId,
                    LessonId = lessonId,
                    AvailableLessons = lessons,
                    PassingScore = 70,
                    MaxAttempts = 3,
                    TimeLimit = 30,
                    IsActive = true,
                    IsTimedQuiz = false
                };

                return Result<CreateEditQuizDto>.Success(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating view model for course {CourseId}", courseId);
                return Result<CreateEditQuizDto>.Fail("Có lỗi xảy ra");
            }
        }

        public async Task<Result<CreateEditQuizDto>> GetEditViewModelAsync(Guid quizId)
        {
            try
            {
                var quiz = await _context.Quizzes
                    .Include(q => q.Questions)
                        .ThenInclude(q => q.Answers)
                    .FirstOrDefaultAsync(q => q.Id == quizId);

                if (quiz == null)
                {
                    return Result<CreateEditQuizDto>.Fail("Không tìm thấy bài kiểm tra");
                }

                var lessons = await GetLessonsForCourseAsync(quiz.CourseId);

                var model = new CreateEditQuizDto
                {
                    QuizId = quiz.Id,
                    CourseId = quiz.CourseId,
                    LessonId = quiz.LessonId,
                    Title = quiz.Title,
                    Description = quiz.Description,
                    TimeLimit = quiz.TimeLimit,
                    MaxAttempts = quiz.MaxAttempts,
                    PassingScore = quiz.PassingScore,
                    IsActive = quiz.IsActive,
                    IsTimedQuiz = quiz.IsTimedQuiz,
                    AvailableLessons = lessons,
                    Questions = MapQuestionsToDto(quiz.Questions)
                };

                return Result<CreateEditQuizDto>.Success(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting edit view model for quiz {QuizId}", quizId);
                return Result<CreateEditQuizDto>.Fail("Có lỗi xảy ra");
            }
        }

        #endregion

        #region Create/Update Quiz

        public async Task<Result<Guid>> CreateQuizAsync(CreateEditQuizDto model)
        {
            try
            {
                var validationResult = await ValidateQuizDataAsync(model);
                if (!validationResult.IsSuccess)
                {
                    return Result<Guid>.Fail(validationResult.ErrorMessage);
                }

                var quiz = new Quiz
                {
                    CourseId = model.CourseId,
                    LessonId = model.LessonId,
                    Title = model.Title,
                    Description = model.Description,
                    TimeLimit = model.TimeLimit,
                    MaxAttempts = model.MaxAttempts,
                    PassingScore = model.PassingScore,
                    IsActive = model.IsActive,
                    IsTimedQuiz = model.IsTimedQuiz,
                    CreatedDate = DateTime.UtcNow
                };

                _context.Quizzes.Add(quiz);
                await _context.SaveChangesAsync();

                if (model.Questions != null && model.Questions.Any())
                {
                    await AddQuestionsToQuizAsync(quiz.Id, model.Questions);
                }

                _logger.LogInformation("Created quiz {QuizId} for course {CourseId}", quiz.Id, model.CourseId);
                return Result<Guid>.Success(quiz.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating quiz for course {CourseId}", model.CourseId);
                return Result<Guid>.Fail("Có lỗi xảy ra khi tạo bài kiểm tra");
            }
        }

        public async Task<Result<Guid>> UpdateQuizAsync(CreateEditQuizDto model)
        {
            try
            {
                if (!model.QuizId.HasValue)
                {
                    return Result<Guid>.Fail("Quiz ID không hợp lệ");
                }

                var validationResult = await ValidateQuizDataAsync(model);
                if (!validationResult.IsSuccess)
                {
                    return Result<Guid>.Fail(validationResult.ErrorMessage);
                }

                var quiz = await _context.Quizzes
                    .Include(q => q.Questions)
                        .ThenInclude(q => q.Answers)
                    .FirstOrDefaultAsync(q => q.Id == model.QuizId.Value);

                if (quiz == null)
                {
                    return Result<Guid>.Fail("Không tìm thấy bài kiểm tra");
                }

                quiz.Title = model.Title;
                quiz.Description = model.Description;
                quiz.LessonId = model.LessonId;
                quiz.TimeLimit = model.TimeLimit;
                quiz.MaxAttempts = model.MaxAttempts;
                quiz.PassingScore = model.PassingScore;
                quiz.IsActive = model.IsActive;
                quiz.IsTimedQuiz = model.IsTimedQuiz;

                _context.Questions.RemoveRange(quiz.Questions);
                await _context.SaveChangesAsync();

                if (model.Questions != null && model.Questions.Any())
                {
                    await AddQuestionsToQuizAsync(quiz.Id, model.Questions);
                }

                _logger.LogInformation("Updated quiz {QuizId}", quiz.Id);
                return Result<Guid>.Success(quiz.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating quiz {QuizId}", model.QuizId);
                return Result<Guid>.Fail("Có lỗi xảy ra khi cập nhật bài kiểm tra");
            }
        }

        #endregion

        #region Delete Quiz

        public async Task<Result<bool>> DeleteQuizAsync(Guid quizId)
        {
            try
            {
                var quiz = await _context.Quizzes
                    .Include(q => q.Questions)
                    .Include(q => q.QuizAttempts)
                    .FirstOrDefaultAsync(q => q.Id == quizId);

                if (quiz == null)
                {
                    return Result<bool>.Fail("Không tìm thấy bài kiểm tra");
                }

                if (quiz.QuizAttempts.Any())
                {
                    return Result<bool>.Fail(
                        "Không thể xóa bài kiểm tra đã có học viên làm bài. Bạn có thể đặt trạng thái 'Không hoạt động' thay vì xóa.");
                }

                _context.Quizzes.Remove(quiz);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted quiz {QuizId}", quizId);
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting quiz {QuizId}", quizId);
                return Result<bool>.Fail("Có lỗi xảy ra khi xóa bài kiểm tra");
            }
        }

        #endregion

        #region Validation

        public async Task<Result<bool>> ValidateQuizDataAsync(CreateEditQuizDto model)
        {
            var courseExists = await _context.Courses.AnyAsync(c => c.Id == model.CourseId);
            if (!courseExists)
            {
                return Result<bool>.Fail("Khóa học không tồn tại");
            }

            if (model.LessonId.HasValue)
            {
                var lesson = await _context.Lessons
                    .FirstOrDefaultAsync(l => l.Id == model.LessonId.Value && l.CourseId == model.CourseId);

                if (lesson == null)
                {
                    return Result<bool>.Fail("Bài học không thuộc khóa học này");
                }

                var existingQuiz = await _context.Quizzes
                    .Where(q => q.LessonId == model.LessonId.Value)
                    .Where(q => !model.QuizId.HasValue || q.Id != model.QuizId.Value)
                    .AnyAsync();

                if (existingQuiz)
                {
                    return Result<bool>.Fail("Bài học này đã có bài kiểm tra");
                }
            }

            if (model.Questions != null && model.Questions.Any())
            {
                foreach (var question in model.Questions)
                {
                    if (string.IsNullOrWhiteSpace(question.Content))
                    {
                        return Result<bool>.Fail("Nội dung câu hỏi không được để trống");
                    }

                    if (question.Points < 1 || question.Points > 100)
                    {
                        return Result<bool>.Fail("Điểm câu hỏi phải từ 1-100");
                    }

                    if (question.Type == 0) 
                    {
                        if (question.Answers == null || question.Answers.Count < 2)
                        {
                            return Result<bool>.Fail("Câu hỏi trắc nghiệm phải có ít nhất 2 đáp án");
                        }

                        if (!question.Answers.Any(a => a.IsCorrect))
                        {
                            return Result<bool>.Fail("Câu hỏi trắc nghiệm phải có ít nhất 1 đáp án đúng");
                        }
                    }
                }
            }

            return Result<bool>.Success(true);
        }

        #endregion

        #region Helper Methods

        private async Task<List<LessonSelectItem>> GetLessonsForCourseAsync(Guid courseId)
        {
            return await _context.Lessons
                .Where(l => l.CourseId == courseId)
                .OrderBy(l => l.OrderIndex)
                .Select(l => new LessonSelectItem
                {
                    Id = l.Id,
                    Title = l.Title,
                    OrderIndex = l.OrderIndex
                })
                .ToListAsync();
        }

        private async Task AddQuestionsToQuizAsync(Guid quizId, List<QuestionDto> questions)
        {
            int orderIndex = 1;

            foreach (var questionDto in questions)
            {
                var question = new Question
                {
                    QuizId = quizId,
                    Content = questionDto.Content,
                    Type = (QuestionType)questionDto.Type,
                    Points = questionDto.Points,
                    Explanation = questionDto.Explanation,
                    OrderIndex = orderIndex++
                };

                _context.Questions.Add(question);
                await _context.SaveChangesAsync();
                if (questionDto.Answers != null && questionDto.Answers.Any())
                {
                    foreach (var answerDto in questionDto.Answers)
                    {
                        var answer = new Answer
                        {
                            QuestionId = question.Id,
                            Content = answerDto.Content,
                            IsCorrect = answerDto.IsCorrect,
                            OrderIndex = answerDto.OrderIndex
                        };

                        _context.Answers.Add(answer);
                    }

                    await _context.SaveChangesAsync();
                }
                
            }
        }

        private QuizDetailDto MapToQuizDetailDto(Quiz quiz)
        {
            return new QuizDetailDto
            {
                Id = quiz.Id,
                CourseId = quiz.CourseId,
                CourseName = quiz.Course?.Title,
                LessonId = quiz.LessonId,
                LessonName = quiz.Lesson?.Title,
                Title = quiz.Title,
                Description = quiz.Description,
                TimeLimit = quiz.TimeLimit,
                MaxAttempts = quiz.MaxAttempts,
                PassingScore = quiz.PassingScore,
                IsActive = quiz.IsActive,
                IsTimedQuiz = quiz.IsTimedQuiz,
                CreatedDate = quiz.CreatedDate,
                TotalQuestions = quiz.Questions.Count,
                TotalPoints = quiz.Questions.Sum(q => q.Points),
                Questions = MapQuestionsToDto(quiz.Questions),
                TotalAttempts = quiz.QuizAttempts.Count
            };
        }

        private List<QuestionDto> MapQuestionsToDto(ICollection<Question> questions)
        {
            return questions
                .OrderBy(q => q.OrderIndex)
                .Select(q => new QuestionDto
                {
                    Id = q.Id.ToString(),
                    Type = (int)q.Type,
                    Content = q.Content,
                    Points = q.Points,
                    Explanation = q.Explanation,
                    Answers = q.Answers
                        .OrderBy(a => a.OrderIndex)
                        .Select(a => new AnswerDto
                        {
                            Content = a.Content,
                            IsCorrect = a.IsCorrect
                        })
                        .ToList()
                })
                .ToList();
        }

        #endregion
    }
}
