using SmartLMS.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SmartLMS.Core.Entities
{
    public class Course
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ShortDescription { get; set; } = string.Empty;
        public string? Thumbnail { get; set; }
        public decimal Price { get; set; }
        public bool IsFree { get; set; }
        public CourseLevel Level { get; set; } = CourseLevel.Beginner;
        public string? Requirements { get; set; }
        public string? WhatYouWillLearn { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        // FK
        public int CategoryId { get; set; }
        public Guid InstructorId { get; set; }

        // Tracking
        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public CourseStatus Status { get; set; } = CourseStatus.Draft;
        public string? RejectReason { get; set; }
        public DateTime? RevisionDeadline { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? ArchivedAt { get; set; }

        // Navigation
        public Category Category { get; set; } = null!;
        public ApplicationUser Instructor { get; set; } = null!;
        public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();

        // Calculated
        public int TotalLessons => Lessons.Count;
        public int TotalEnrollments => Enrollments.Count;
    }
}
