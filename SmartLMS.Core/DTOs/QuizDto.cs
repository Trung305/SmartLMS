using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLMS.Core.DTOs
{
    public class QuizDto
    {
        public Guid Id { get; set; }
        public Guid CourseId { get; set; }
        public Guid? LessonId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int TimeLimit { get; set; }
        public int MaxAttempts { get; set; }
        public decimal PassingScore { get; set; }
        public bool IsActive { get; set; }
        public bool IsTimedQuiz { get; set; }
        public DateTime CreatedDate { get; set; }
        public int TotalQuestions { get; set; }
        public int TotalPoints { get; set; }
    }
    public class QuizDetailDto : QuizDto
    {
        public string? CourseName { get; set; }
        public string? LessonName { get; set; }
        public List<QuestionDto> Questions { get; set; } = new();
        public int TotalAttempts { get; set; }
    }
    public class CreateEditQuizDto
    {
        public Guid? QuizId { get; set; }
        public Guid CourseId { get; set; }
        public Guid? LessonId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int TimeLimit { get; set; } = 30;
        public int MaxAttempts { get; set; } = 3;
        public decimal PassingScore { get; set; } = 70;
        public bool IsActive { get; set; } = true;
        public bool IsTimedQuiz { get; set; } = false;
        public List<LessonSelectItem>? AvailableLessons { get; set; }
        public List<QuestionDto>? Questions { get; set; }
    }
    public class LessonSelectItem
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
    }
}
