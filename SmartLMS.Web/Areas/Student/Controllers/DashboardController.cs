using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartLMS.Core.Common;
using SmartLMS.Core.Entities;
using SmartLMS.Infrastructure.Data;
using SmartLMS.Web.Areas.Student.Models;

namespace SmartLMS.Web.Areas.Student.Controllers;

[Area("Student")]
[Authorize(Roles = Constants.STUDENT_ROLE)]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<DashboardController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        var enrollments = await _context.Enrollments
            .Where(e => e.StudentId == user.Id)
            .Include(e => e.Course)
                .ThenInclude(c => c.Category)
            .Include(e => e.Course)
                .ThenInclude(c => c.Instructor)
            .Include(e => e.LessonProgress)
            .OrderByDescending(e => e.EnrollmentDate)
            .ToListAsync();

        var recentActivity = await _context.LessonProgress
            .Where(lp => lp.Enrollment.StudentId == user.Id)
            .Include(lp => lp.Lesson)
                .ThenInclude(l => l.Course)
            .OrderByDescending(lp => lp.LastAccessDate)
            .Take(10)
            .ToListAsync();

        var viewModel = new StudentDashboardViewModel
        {
            Student = user,
            TotalEnrollments = enrollments.Count,
            CompletedCourses = enrollments.Count(e => e.Progress >= 100),
            InProgressCourses = enrollments.Count(e => e.Progress > 0 && e.Progress < 100),
            RecentEnrollments = enrollments.Take(6).ToList(),
            RecentActivity = recentActivity,
            OverallProgress = enrollments.Any() ? enrollments.Average(e => e.Progress) : 0
        };

        return View(viewModel);
    }

    public async Task<IActionResult> MyCourses(int page = 1)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        var pageSize = 12;
        var query = _context.Enrollments
            .Where(e => e.StudentId == user.Id)
            .Include(e => e.Course)
                .ThenInclude(c => c.Category)
            .Include(e => e.Course)
                .ThenInclude(c => c.Instructor)
            .AsQueryable();

        var totalCount = await query.CountAsync();
        var enrollments = await query
            .OrderByDescending(e => e.EnrollmentDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var viewModel = new StudentCoursesViewModel
        {
            Enrollments = new PagedResult<Enrollment>
            {
                Items = enrollments,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            }
        };

        return View(viewModel);
    }

    public async Task<IActionResult> CourseProgress(Guid courseId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        var enrollment = await _context.Enrollments
            .Where(e => e.StudentId == user.Id && e.CourseId == courseId)
            .Include(e => e.Course)
                .ThenInclude(c => c.Lessons.OrderBy(l => l.OrderIndex))
            .Include(e => e.LessonProgress)
                .ThenInclude(lp => lp.Lesson)
            .FirstOrDefaultAsync();

        if (enrollment == null)
        {
            return NotFound();
        }

        return View(enrollment);
    }
}