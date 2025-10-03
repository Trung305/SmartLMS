using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartLMS.Core.Entities;
using SmartLMS.Core.Enums;
using SmartLMS.Infrastructure.Data;

namespace SmartLMS.Web.Controllers;

[Authorize]
public class EnrollmentController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<EnrollmentController> _logger;

    public EnrollmentController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<EnrollmentController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enroll(Guid courseId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        // Kiểm tra khóa học có tồn tại và được publish không
        var course = await _context.Courses
            .Include(c => c.Instructor)
            .FirstOrDefaultAsync(c => c.Id == courseId);

        if (course == null)
        {
            TempData["ErrorMessage"] = "Khóa học không tồn tại.";
            return RedirectToAction("Index", "Courses");
        }

        if (!course.IsPublished)
        {
            TempData["ErrorMessage"] = "Khóa học chưa được xuất bản.";
            return RedirectToAction("Details", "Courses", new { id = courseId });
        }

        // Kiểm tra đã đăng ký chưa
        var existingEnrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.StudentId == user.Id && e.CourseId == courseId);

        if (existingEnrollment != null)
        {
            TempData["InfoMessage"] = "Bạn đã đăng ký khóa học này rồi.";
            return RedirectToAction("Course", "Learning", new { area = "Student", id = courseId });
        }

        // Không cho instructor đăng ký khóa học của chính mình
        if (course.InstructorId == user.Id)
        {
            TempData["ErrorMessage"] = "Bạn không thể đăng ký khóa học của chính mình.";
            return RedirectToAction("Details", "Courses", new { id = courseId });
        }

        // Tạo enrollment mới
        var enrollment = new Enrollment
        {
            StudentId = user.Id,
            CourseId = courseId,
            EnrollmentDate = DateTime.UtcNow,
            Status = EnrollmentStatus.Active
        };

        _context.Enrollments.Add(enrollment);

        try
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("User {UserId} enrolled in course {CourseId}", user.Id, courseId);

            TempData["SuccessMessage"] = "Đăng ký khóa học thành công! Chào mừng bạn đến với khóa học.";

            // Chuyển hướng đến trang học
            return RedirectToAction("Course", "Learning", new { area = "Student", id = courseId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enrolling user {UserId} in course {CourseId}", user.Id, courseId);
            TempData["ErrorMessage"] = "Có lỗi xảy ra khi đăng ký khóa học. Vui lòng thử lại.";
            return RedirectToAction("Details", "Courses", new { id = courseId });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unenroll(Guid courseId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        var enrollment = await _context.Enrollments
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.StudentId == user.Id && e.CourseId == courseId);

        if (enrollment == null)
        {
            TempData["ErrorMessage"] = "Bạn chưa đăng ký khóa học này.";
            return RedirectToAction("Details", "Courses", new { id = courseId });
        }

        // Không cho hủy đăng ký nếu đã hoàn thành > 50%
        if (enrollment.Progress > 50)
        {
            TempData["ErrorMessage"] = "Không thể hủy đăng ký khóa học đã học quá 50%.";
            return RedirectToAction("CourseProgress", "Dashboard", new { area = "Student", courseId });
        }

        enrollment.Status = EnrollmentStatus.Dropped;

        try
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("User {UserId} unenrolled from course {CourseId}", user.Id, courseId);

            TempData["InfoMessage"] = "Đã hủy đăng ký khóa học thành công.";
            return RedirectToAction("MyCourses", "Dashboard", new { area = "Student" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unenrolling user {UserId} from course {CourseId}", user.Id, courseId);
            TempData["ErrorMessage"] = "Có lỗi xảy ra khi hủy đăng ký. Vui lòng thử lại.";
            return RedirectToAction("Details", "Courses", new { id = courseId });
        }
    }

    public async Task<IActionResult> CheckEnrollment(Guid courseId)
    {
        if (!User.Identity?.IsAuthenticated == true)
        {
            return Json(new { isEnrolled = false, canEnroll = false, message = "Vui lòng đăng nhập" });
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Json(new { isEnrolled = false, canEnroll = false, message = "Không tìm thấy thông tin người dùng" });
        }

        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.StudentId == user.Id && e.CourseId == courseId);

        var course = await _context.Courses
            .FirstOrDefaultAsync(c => c.Id == courseId);

        return Json(new
        {
            isEnrolled = enrollment != null && enrollment.Status == EnrollmentStatus.Active,
            canEnroll = course != null && course.IsPublished && course.InstructorId != user.Id,
            progress = enrollment?.Progress ?? 0,
            enrollmentDate = enrollment?.EnrollmentDate.ToString("dd/MM/yyyy")
        });
    }
}