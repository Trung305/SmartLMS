using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartLMS.Core.Entities;
using SmartLMS.Core.Enums;
using SmartLMS.Core.Interfaces.Services;
using SmartLMS.Infrastructure.Data;
using System.Security.Claims;

namespace SmartLMS.Web.Controllers;

[Authorize]
public class EnrollmentController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<EnrollmentController> _logger;
    private readonly IEnrollmentService _enrollmentService;

    public EnrollmentController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IEnrollmentService enrollmentService,
        ILogger<EnrollmentController> logger)
    {
        _context = context;
        _userManager = userManager;
        _enrollmentService = enrollmentService;
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

        var result = await _enrollmentService.EnrollAsync(user.Id, courseId);

        if (result.success)
        {
            TempData["SuccessMessage"] = result.message;
            return RedirectToAction("MyCourses");
        }
        else
        {
            TempData["ErrorMessage"] = result.message;
            return RedirectToAction("Details", "Course", new { id = courseId });
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
            canEnroll = course != null && course.Status == CourseStatus.Published && course.InstructorId != user.Id,
            progress = enrollment?.Progress ?? 0,
            enrollmentDate = enrollment?.EnrollmentDate.ToString("dd/MM/yyyy")
        });
    }

    public async Task<IActionResult> MyCourses()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
        }
        var enrollments = await _enrollmentService.GetUserEnrollmentsAsync(user.Id);

        return View(enrollments);
    }
}