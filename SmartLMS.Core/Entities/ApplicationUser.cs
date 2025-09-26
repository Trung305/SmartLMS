using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
namespace SmartLMS.Core.Entities
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}".Trim();
        public string? Avatar { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginDate { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation properties - Quan hệ với các bảng khác
        public ICollection<Course> CreatedCourses { get; set; } = new List<Course>();        // Khóa học đã tạo (cho Instructor)
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();   // Khóa học đã đăng ký (cho Student)
        public ICollection<QuizAttempt> QuizAttempts { get; set; } = new List<QuizAttempt>(); // Lần làm bài kiểm tra
    }
}
