using SmartLMS.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLMS.Core.DTOs
{
    public class LessonIndexDto
    {
        public Course Course { get; set; } = null!;
        public List<Lesson> Lessons { get; set; } = new();
        public List<QuizDto> Quizzes { get; set; } = new();
    }

    public class LessonCreateEditDto
    {
        public Guid Id { get; set; }
        public Guid CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int Duration { get; set; }
        public int OrderIndex { get; set; }
        public bool IsPreview { get; set; } = false;
        public bool IsPublished { get; set; } = true;
        public string? CurrentVideoUrl { get; set; }
    }

    public class LessonDetailDto
    {
        public Guid Id { get; set; }
        public Guid CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? VideoUrl { get; set; }
        public int Duration { get; set; }
        public int OrderIndex { get; set; }
        public bool IsPreview { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedDate { get; set; }

        public Course Course { get; set; } = null!;

        public int TotalStudentsCompleted { get; set; }
        public double CompletionRate { get; set; }
    }
    public class LessonReorderDto
    {
        public Guid CourseId { get; set; }
        public List<Guid> LessonIds { get; set; } = new();
    }

    public class LessonSimpleDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
        public bool IsPublished { get; set; }
    }
}
