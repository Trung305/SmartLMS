using SmartLMS.Core.DTOs;
using System.ComponentModel.DataAnnotations;

namespace SmartLMS.Web.Areas.Admin.Models
{
    public class AdminCreateEditQuizViewModel
    {
        public Guid? QuizId { get; set; }

        [Required]
        public Guid CourseId { get; set; }

        public Guid? LessonId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tiêu đề bài kiểm tra")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Tiêu đề phải từ 5-200 ký tự")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập thời gian làm bài")]
        [Range(1, 180, ErrorMessage = "Thời gian phải từ 1-180 phút")]
        public int TimeLimit { get; set; } = 30;

        [Required(ErrorMessage = "Vui lòng nhập số lần làm bài")]
        [Range(1, 10, ErrorMessage = "Số lần làm bài phải từ 1-10")]
        public int MaxAttempts { get; set; } = 3;

        [Required(ErrorMessage = "Vui lòng nhập điểm đậu")]
        [Range(0, 100, ErrorMessage = "Điểm đậu phải từ 0-100")]
        public decimal PassingScore { get; set; } = 70;

        public bool IsActive { get; set; } = true;

        public bool IsTimedQuiz { get; set; } = false;
        public List<LessonSelectItem>? AvailableLessons { get; set; }
        public List<QuestionDto>? Questions { get; set; }
    }
}
