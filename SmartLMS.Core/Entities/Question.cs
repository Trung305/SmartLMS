using SmartLMS.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLMS.Core.Entities
{
    public class Question
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid QuizId { get; set; }
        public string Content { get; set; } = string.Empty;    // Nội dung câu hỏi
        public QuestionType Type { get; set; } = QuestionType.MultipleChoice;
        public int Points { get; set; } = 1;                   // Điểm cho câu hỏi
        public int OrderIndex { get; set; }                    // Thứ tự câu hỏi
        public string? Explanation { get; set; }               // Giải thích đáp án

        // Navigation properties
        public Quiz Quiz { get; set; } = null!;
        public ICollection<Answer> Answers { get; set; } = new List<Answer>();
    }
}
