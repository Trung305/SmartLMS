using SmartLMS.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLMS.Core.Entities
{
    public class Enrollment
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid StudentId { get; set; }
        public Guid CourseId { get; set; }
        public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
       // public bool IsCompleted { get; set; } = false;
        //public DateTime LastAccessedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletionDate { get; set; }           
        public decimal Progress { get; set; } = 0;              
        public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Active;
        public decimal? FinalScore { get; set; }                

        // Navigation properties
        public ApplicationUser Student { get; set; } = null!;
        public Course Course { get; set; } = null!;
        public ICollection<LessonProgress> LessonProgress { get; set; } = new List<LessonProgress>();

        // Calculated properties
        public bool IsCompleted => Status == EnrollmentStatus.Completed && CompletionDate.HasValue;
        public int DaysEnrolled => (DateTime.UtcNow - EnrollmentDate).Days;
    }

}
