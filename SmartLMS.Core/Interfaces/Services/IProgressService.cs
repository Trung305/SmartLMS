using SmartLMS.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLMS.Core.Interfaces.Services
{
    public interface IProgressService
    {
        Task<LessonProgress> GetProgressAsync(Guid userId, Guid lessonId);
        Task<bool> UpdateProgressAsync(Guid userId, VideoProgressUpdateDto dto);
        Task<bool> MarkAsCompletedAsync(Guid userId, Guid lessonId);
        Task<decimal> CalculateCourseProgressAsync(Guid userId, Guid courseId);
    }
    public class VideoProgressUpdateDto
    {
        public Guid LessonId { get; set; }
        public int CurrentTime { get; set; } 
        public int Duration { get; set; }
        public bool IsCompleted { get; set; }
    }
}
