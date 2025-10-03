using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartLMS.Infrastructure.Data;
using SmartLMS.Web.Models.ViewModels;

namespace SmartLMS.Web.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var viewModel = new HomeViewModel
        {
            FeaturedCourses = await _context.Courses
                .Where(c => c.IsPublished)
                .Include(c => c.Category)
                .Include(c => c.Instructor)
                .OrderByDescending(c => c.CreatedDate)
                .Take(6)
                .ToListAsync(),

            Categories = await _context.Categories
                .Where(c => c.IsActive && c.ParentCategoryId == null)
                .Take(8)
                .ToListAsync(),

            Stats = new HomeStatsViewModel
            {
                TotalCourses = await _context.Courses.CountAsync(c => c.IsPublished),
                TotalStudents = await _context.Users.CountAsync(),
                TotalInstructors = await _context.UserRoles
                    .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur, r })
                    .CountAsync(x => x.r.Name == "Instructor")
            }
        };

        return View(viewModel);
    }

    public IActionResult About()
    {
        return View();
    }

    public IActionResult Contact()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}