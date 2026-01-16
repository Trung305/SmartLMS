using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartLMS.Core.Common;
using SmartLMS.Infrastructure.Data;
using SmartLMS.Web.Areas.Admin.Models;

namespace SmartLMS.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = Constants.ADMIN_ROLE)]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(ApplicationDbContext context, ILogger<DashboardController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var viewModel = new AdminDashboardViewModel
        {
            TotalCourses = await _context.Courses.CountAsync(),
            TotalUsers = await _context.Users.CountAsync(),
            TotalEnrollments = await _context.Enrollments.CountAsync(),
            TotalActiveUsers = await _context.Users.CountAsync(u => u.IsActive),

            RecentCourses = await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Instructor)
                .OrderByDescending(c => c.CreatedAt)
                .Take(5)
                .ToListAsync(),

            RecentEnrollments = await _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Course)
                .OrderByDescending(e => e.EnrollmentDate)
                .Take(10)
                .ToListAsync(),

            MonthlyStats = await GetMonthlyStatsAsync()
        };

        return View(viewModel);
    }

    private async Task<List<MonthlyStatModel>> GetMonthlyStatsAsync()
    {
        var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);

        var enrollmentsByMonth = await _context.Enrollments
            .Where(e => e.EnrollmentDate >= sixMonthsAgo)
            .GroupBy(e => new { e.EnrollmentDate.Year, e.EnrollmentDate.Month })
            .Select(g => new MonthlyStatModel
            {
                Month = g.Key.Month,
                Year = g.Key.Year,
                Enrollments = g.Count(),
                Revenue = g.Sum(e => e.Course.Price)
            })
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month)
            .ToListAsync();

        return enrollmentsByMonth;
    }

    public async Task<IActionResult> GetChartData()
    {
        var stats = await GetMonthlyStatsAsync();

        var result = new
        {
            labels = stats.Select(s => $"Tháng {s.Month}/{s.Year}").ToArray(),
            enrollments = stats.Select(s => s.Enrollments).ToArray(),
            revenue = stats.Select(s => s.Revenue).ToArray()
        };

        return Json(result);
    }
}