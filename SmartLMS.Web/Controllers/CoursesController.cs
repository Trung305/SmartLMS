using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartLMS.Core.Common;
using SmartLMS.Core.Enums;
using SmartLMS.Infrastructure.Data;
using SmartLMS.Web.Models.ViewModels;

namespace SmartLMS.Web.Controllers;

public class CoursesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CoursesController> _logger;

    public CoursesController(ApplicationDbContext context, ILogger<CoursesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> Index(int? categoryId, string? level, string? search, int page = 1)
    {
        var pageSize = Constants.DEFAULT_PAGE_SIZE;

        var query = _context.Courses
            .Where(c => c.Status == CourseStatus.Published)
            .Include(c => c.Category)
            .Include(c => c.Instructor)
            .AsQueryable();

        // Apply filters
        if (categoryId.HasValue)
        {
            query = query.Where(c => c.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrEmpty(level))
        {
            if (Enum.TryParse<SmartLMS.Core.Enums.CourseLevel>(level, out var courseLevel))
            {
                query = query.Where(c => c.Level == courseLevel);
            }
        }

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(c =>
                c.Title.Contains(search) ||
                c.Description.Contains(search) ||
                c.ShortDescription.Contains(search));
        }

        // Get total count for pagination
        var totalCount = await query.CountAsync();

        // Apply pagination and ordering
        var courses = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var viewModel = new CourseIndexViewModel
        {
            Courses = new PagedResult<SmartLMS.Core.Entities.Course>
            {
                Items = courses,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            },
            Categories = await _context.Categories
                .Where(c => c.IsActive && c.ParentCategoryId == null)
                .ToListAsync(),
            SelectedCategoryId = categoryId,
            SelectedLevel = level,
            SearchQuery = search
        };

        return View(viewModel);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var course = await _context.Courses
            .Include(c => c.Category)
            .Include(c => c.Instructor)
            .Include(c => c.Lessons.Where(l => l.IsPublished))
            .FirstOrDefaultAsync(c => c.Id == id);

        if (course == null)
        {
            return NotFound();
        }

        var viewModel = new CourseDetailsViewModel
        {
            Course = course,
            IsEnrolled = false // TODO: Check if current user is enrolled
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Search(string query)
    {
        if (string.IsNullOrEmpty(query))
        {
            return Json(new { success = false, message = "Từ khóa tìm kiếm không được để trống" });
        }

        var courses = await _context.Courses
            .Where(c => c.Status == CourseStatus.Published &&
                (c.Title.Contains(query) || c.Description.Contains(query)))
            .Include(c => c.Category)
            .Include(c => c.Instructor)
            .Take(10)
            .Select(c => new
            {
                id = c.Id,
                title = c.Title,
                thumbnail = c.Thumbnail,
                price = c.Price,
                isFree = c.IsFree,
                level = c.Level.ToString(),
                category = c.Category.Name,
                instructor = c.Instructor.FullName
            })
            .ToListAsync();

        return Json(new { success = true, data = courses });
    }
}