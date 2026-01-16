using Microsoft.EntityFrameworkCore;
using SmartLMS.Core.Entities;
using SmartLMS.Core.Enums;
using SmartLMS.Core.Interfaces.Services;
using SmartLMS.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLMS.Infrastructure.Services
{
    public class ProgressService : IProgressService
    {
        private readonly ApplicationDbContext _context;
        public ProgressService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<LessonProgress> GetProgressAsync(Guid userId, Guid lessonId)
        {
            LessonProgress progress = null;
            try
            {
                var enrollment = await _context.Enrollments
                .Include(e => e.LessonProgress)
                .ThenInclude(lp => lp.Lesson)
                .FirstOrDefaultAsync(e =>
                    e.StudentId == userId &&
                    e.Status == EnrollmentStatus.Active &&
                    e.Course.Lessons.Any(l => l.Id == lessonId));

                if (enrollment == null)
                    return null;

                progress = enrollment.LessonProgress
                    .FirstOrDefault(lp => lp.LessonId == lessonId);

                if (progress == null)
                {
                    var lesson = await _context.Lessons.FindAsync(lessonId);
                    if (lesson == null)
                        return null;

                    progress = new LessonProgress
                    {
                        Id = Guid.NewGuid(),
                        EnrollmentId = enrollment.Id,
                        LessonId = lessonId,
                        IsCompleted = false,
                        WatchedDuration = 0,
                        FirstWatchedDate = DateTime.UtcNow,
                        LastAccessDate = DateTime.UtcNow,
                        Lesson = lesson
                    };
                    _context.LessonProgress.Add(progress);
                    await _context.SaveChangesAsync();
                }

                
            }
            catch(Exception Ex)
            {

            }
            return progress;
        }
        public async Task<bool> UpdateProgressAsync(Guid userId, VideoProgressUpdateDto dto)
        {
            try
            {
                var progress = await _context.LessonProgress
                    .Include(lp => lp.Lesson)
                    .Include(lp => lp.Enrollment)
                    .FirstOrDefaultAsync(lp =>
                        lp.LessonId == dto.LessonId &&
                        lp.Enrollment.StudentId == userId);

                if (progress == null)
                {
                    progress = await GetProgressAsync(userId, dto.LessonId);
                    if (progress == null)
                        return false;
                }

                progress.WatchedDuration = dto.CurrentTime;
                progress.LastAccessDate = DateTime.UtcNow;

                if (!progress.FirstWatchedDate.HasValue)
                {
                    progress.FirstWatchedDate = DateTime.UtcNow;
                }

                var watchPercentage = progress.Lesson?.Duration > 0
                    ? (decimal)dto.CurrentTime / progress.Lesson.Duration * 100
                    : 0;

                if (watchPercentage >= 90 && !progress.IsCompleted)
                {
                    progress.IsCompleted = true;
                    progress.CompletedDate = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                if (progress.Enrollment != null)
                {
                    var courseProgress = await CalculateCourseProgressAsync(
                        userId,
                        progress.Enrollment.CourseId
                    );

                    progress.Enrollment.Progress = courseProgress;
                    progress.LastAccessDate = DateTime.UtcNow;

                    if (courseProgress >= 100 && !progress.Enrollment.IsCompleted)
                    {
                        progress.IsCompleted = true;
                        progress.Enrollment.CompletionDate = DateTime.UtcNow;
                    }

                    await _context.SaveChangesAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public async Task<bool> MarkAsCompletedAsync(Guid userId, Guid lessonId)
        {
            var progress = await _context.LessonProgress
                .Include(lp => lp.Enrollment)
                .FirstOrDefaultAsync(lp =>
                    lp.LessonId == lessonId &&
                    lp.Enrollment.StudentId == userId);

            if (progress == null)
                return false;

            progress.IsCompleted = true;
            progress.CompletedDate = DateTime.UtcNow;
            progress.LastAccessDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await CalculateCourseProgressAsync(userId, progress.Enrollment.CourseId);

            return true;
        }
        public async Task<decimal> CalculateCourseProgressAsync(Guid userId, Guid courseId)
        {
            var enrollment = await _context.Enrollments
                .Include(e => e.LessonProgress)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Lessons)
                .FirstOrDefaultAsync(e =>
                    e.StudentId == userId &&
                    e.CourseId == courseId &&
                    e.Status == EnrollmentStatus.Active);

            if (enrollment == null || !enrollment.Course.Lessons.Any())
                return 0;

            var totalLessons = enrollment.Course.Lessons.Count;
            var completedLessons = enrollment.LessonProgress
                .Count(lp => lp.IsCompleted);

            var progress = (decimal)completedLessons / totalLessons * 100;

            // Update enrollment progress
            enrollment.Progress = progress;
            await _context.SaveChangesAsync();

            return progress;
        }
    }
}
