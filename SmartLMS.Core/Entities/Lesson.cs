using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLMS.Core.Entities
{
    public class Lesson
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;     // Nội dung text của bài học
        public string? VideoUrl { get; set; }                   // Đường dẫn video
        public int Duration { get; set; }                       // Thời lượng video (giây)
        public int OrderIndex { get; set; }                     // Thứ tự bài học trong khóa
        public bool IsPreview { get; set; } = false;           // Có cho phép xem trước không
        public bool IsPublished { get; set; } = true;          // Đã xuất bản chưa
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Course Course { get; set; } = null!;
        public ICollection<LessonProgress> LessonProgress { get; set; } = new List<LessonProgress>();
    }
}
