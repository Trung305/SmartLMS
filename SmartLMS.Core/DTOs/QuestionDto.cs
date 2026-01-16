using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLMS.Core.DTOs
{
    public class QuestionDto
    {
        public string? Id { get; set; }
        public int Type { get; set; }
        [Required(ErrorMessage = "Nội dung câu hỏi không được để trống")]
        public string Content { get; set; } = string.Empty;
        [Range(1, 100, ErrorMessage = "Điểm phải từ 1-100")]
        public int Points { get; set; } = 1;
        [StringLength(1000, ErrorMessage = "Giải thích không được vượt quá 1000 ký tự")]
        public string? Explanation { get; set; }
        public List<AnswerDto>? Answers { get; set; }
    }
}
