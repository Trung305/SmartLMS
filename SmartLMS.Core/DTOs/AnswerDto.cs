using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLMS.Core.DTOs
{
    public class AnswerDto
    {
        public Guid Id { get; set; }
        public Guid QuestionId { get; set; }
        [Required(ErrorMessage = "Nội dung đáp án không được để trống")]
        [StringLength(500, ErrorMessage = "Đáp án không được vượt quá 500 ký tự")]
        public string Content { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public int OrderIndex { get; set; }
    }
}
