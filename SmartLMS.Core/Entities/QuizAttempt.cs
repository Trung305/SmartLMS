using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLMS.Core.Entities
{
    public class QuizAttempt
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid QuizId { get; set; }
        public Guid StudentId { get; set; }
        public decimal Score { get; set; } = 0;                // Điểm đạt được
        public decimal MaxScore { get; set; }                  // Điểm tối đa
        public int AttemptNumber { get; set; } = 1;            // Lần thử thứ mấy
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        public bool IsCompleted { get; set; } = false;
        public string? StudentAnswers { get; set; }            // JSON chứa đáp án của học viên

        // Navigation properties
        public Quiz Quiz { get; set; } = null!;
        public ApplicationUser Student { get; set; } = null!;

        // Calculated properties
        public bool IsPassed => IsCompleted && Score >= Quiz.PassingScore;
        public decimal ScorePercentage => MaxScore > 0 ? Score / MaxScore * 100 : 0;
        public TimeSpan? Duration => CompletedAt.HasValue ? CompletedAt.Value - StartedAt : null;
    }
}
