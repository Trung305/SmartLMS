using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLMS.Core.Entities
{
    public class Answer
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid QuestionId { get; set; }
        public string Content { get; set; } = string.Empty;    // Nội dung đáp án
        public bool IsCorrect { get; set; } = false;          // Đáp án đúng
        public int OrderIndex { get; set; }                    // Thứ tự đáp án

        // Navigation properties
        public Question Question { get; set; } = null!;
    }
}
