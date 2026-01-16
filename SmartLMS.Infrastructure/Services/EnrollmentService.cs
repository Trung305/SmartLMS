using SmartLMS.Core.Entities;
using SmartLMS.Core.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartLMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using SmartLMS.Core.Enums;
using Microsoft.AspNetCore.Identity;

namespace SmartLMS.Core.Services
{
    public class EnrollmentService : IEnrollmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public EnrollmentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(bool success, string message)> EnrollAsync(Guid userId, Guid courseId)
        {
            // Kiểm tra khóa học có tồn tại và được publish không
            var course = await _context.Courses
                .Include(c => c.Instructor)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
            {
                return (true, "Khóa học không tồn tại.");
            }

            if (course.Status != CourseStatus.Published)
            {
                return (true, "Khóa học chưa được xuất bản.");
            }

            // Kiểm tra đã đăng ký chưa
            var existingEnrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.StudentId == userId && e.CourseId == courseId);

            if (existingEnrollment != null)
            {
                return (true, "Bạn đã đăng ký khóa học này rồi.");
            }

            // Không cho instructor đăng ký khóa học của chính mình
            if (course.InstructorId == userId)
            {
                return (false, "Bạn không thể đăng ký khóa học của chính mình.");
            }

            // Tạo enrollment mới
            var enrollment = new Enrollment
            {
                StudentId = userId,
                CourseId = courseId,
                EnrollmentDate = DateTime.UtcNow,
                Status = EnrollmentStatus.Active
            };

            _context.Enrollments.Add(enrollment);

            try
            {
                await _context.SaveChangesAsync();

                return (true, "Đăng ký khóa học thành công! Chào mừng bạn đến với khóa học.");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi: {ex.ToString()}");
            }
        }

        public async Task<bool> IsEnrolledAsync(Guid userId, Guid courseId)
        {
            return await _context.Enrollments
                .AnyAsync(e => e.StudentId == userId && e.CourseId == courseId && e.Status == EnrollmentStatus.Active);
        }

        public async Task<IEnumerable<Enrollment>> GetUserEnrollmentsAsync(Guid userId)
        {
            return await _context.Enrollments
                .Include(e => e.Course)
                    .ThenInclude(c => c.Category)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Instructor)
                .Where(e => e.StudentId == userId && e.Status == EnrollmentStatus.Active)
                .OrderByDescending(e => e.EnrollmentDate)
                .ToListAsync();
        }

        public async Task<Enrollment> GetEnrollmentAsync(Guid userId, Guid courseId)
        {
            return await _context.Enrollments
                .Include(e => e.Course)
                .Include(e => e.LessonProgress)
                .FirstOrDefaultAsync(e => e.StudentId == userId && e.CourseId == courseId && e.Status == EnrollmentStatus.Active);
        }

        public async Task<bool> UpdateProgressAsync(Guid enrollmentId, decimal progress)
        {
            var enrollment = await _context.Enrollments.FindAsync(enrollmentId);
            if (enrollment == null) return false;

            enrollment.Progress = progress;
            //enrollment.LastAccessedDate = DateTime.Now;

            if (progress >= 100)
            {
                //enrollment.IsCompleted = true;
                enrollment.CompletionDate = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetEnrolledCountAsync(Guid courseId)
        {
            return await _context.Enrollments
                .CountAsync(e => e.CourseId == courseId && e.Status == EnrollmentStatus.Active);
        }
    }
}
