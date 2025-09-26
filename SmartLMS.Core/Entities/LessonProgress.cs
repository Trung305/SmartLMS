using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLMS.Core.Entities
{
    public class LessonProgress
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid EnrollmentId { get; set; }
        public Guid LessonId { get; set; }
        public bool IsCompleted { get; set; } = false;
        public int WatchedDuration { get; set; } = 0;           // Thời gian đã xem (giây)
        public DateTime? FirstWatchedDate { get; set; }         // Lần đầu xem
        public DateTime? CompletedDate { get; set; }            // Ngày hoàn thành
        public DateTime LastAccessDate { get; set; } = DateTime.UtcNow; // Lần truy cập cuối

        // Navigation properties
        public Enrollment Enrollment { get; set; } = null!;
        public Lesson Lesson { get; set; } = null!;

        // Calculated properties
        public decimal WatchPercentage => Lesson?.Duration > 0 ?
            Math.Min(100, (decimal)WatchedDuration / Lesson.Duration * 100) : 0;
    }
}
