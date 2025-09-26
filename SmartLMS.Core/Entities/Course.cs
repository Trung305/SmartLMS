using SmartLMS.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLMS.Core.Entities
{
    public class Course
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ShortDescription { get; set; } = string.Empty;
        public string? Thumbnail { get; set; }           // Hình đại diện khóa học
        public decimal Price { get; set; }
        public bool IsFree { get; set; }
        public CourseLevel Level { get; set; } = CourseLevel.Beginner;
        public int EstimatedDuration { get; set; }       // Thời gian ước tính (phút)
        public string? Requirements { get; set; }        // Yêu cầu đầu vào
        public string? WhatYouWillLearn { get; set; }    // Bạn sẽ học được gì

        // Foreign keys
        public int CategoryId { get; set; }
        public Guid InstructorId { get; set; }

        // Status
        public bool IsPublished { get; set; } = false;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? PublishedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        // Navigation properties
        public Category Category { get; set; } = null!;
        public ApplicationUser Instructor { get; set; } = null!;
        public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();

        // Calculated properties
        public int TotalLessons => Lessons.Count;
        public int TotalEnrollments => Enrollments.Count;
    }
}
