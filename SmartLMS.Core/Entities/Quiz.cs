using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLMS.Core.Entities
{
    public class Quiz
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid CourseId { get; set; }
        public Guid? LessonId { get; set; }                     // Null nếu là quiz tổng kết khóa học
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int TimeLimit { get; set; }                      // Thời gian làm bài (phút)
        public int MaxAttempts { get; set; } = 3;              // Số lần làm tối đa
        public decimal PassingScore { get; set; } = 70;        // Điểm đậu
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Course Course { get; set; } = null!;
        public Lesson? Lesson { get; set; }
        public ICollection<Question> Questions { get; set; } = new List<Question>();
        public ICollection<QuizAttempt> QuizAttempts { get; set; } = new List<QuizAttempt>();

        // Calculated properties
        public int TotalQuestions => Questions.Count;
        public int TotalPoints => Questions.Sum(q => q.Points);
    }
}
