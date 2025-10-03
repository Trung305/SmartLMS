
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartLMS.Core.Common;
using SmartLMS.Core.Entities;
using SmartLMS.Infrastructure.Data;
using SmartLMS.Web.Areas.Admin.Models;

namespace SmartLMS.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = Constants.ADMIN_ROLE)]
public class CoursesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CoursesController> _logger;
    private readonly UserManager<ApplicationUser> _userManager;

    public CoursesController(ApplicationDbContext context, ILogger<CoursesController> logger, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _logger = logger;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(int page = 1, string? search = null)
    {
        var pageSize = 20;
        var query = _context.Courses
            .Include(c => c.Category)
            .Include(c => c.Instructor)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(c => c.Title.Contains(search) ||
                                   c.Instructor.FirstName.Contains(search) ||
                                   c.Instructor.LastName.Contains(search));
        }

        var totalCount = await query.CountAsync();
        var courses = await query
            .OrderByDescending(c => c.CreatedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var viewModel = new AdminCoursesViewModel
        {
            Courses = new PagedResult<SmartLMS.Core.Entities.Course>
            {
                Items = courses,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            },
            SearchQuery = search
        };

        return View(viewModel);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var course = await _context.Courses
            .Include(c => c.Category)
            .Include(c => c.Instructor)
            .Include(c => c.Lessons)
            .Include(c => c.Enrollments)
                .ThenInclude(e => e.Student)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (course == null)
        {
            return NotFound();
        }
        var viewModel = new AdminCourseDetailsViewModel
        {
            Course = course,
            RecentEnrollments = course.Enrollments
           .OrderByDescending(e => e.EnrollmentDate)
           .Take(5) 
           .ToList(),
            Statistics = new CourseStatistics
            {
                TotalEnrollments = course.Enrollments.Count,
                TotalLessons = course.Lessons.Count
            }
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> TogglePublish(Guid id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course == null)
        {
            return NotFound();
        }

        course.IsPublished = !course.IsPublished;
        course.PublishedDate = course.IsPublished ? DateTime.UtcNow : null;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = course.IsPublished ?
            "Khóa học đã được xuất bản." :
            "Khóa học đã được ẩn.";

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(Guid id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course == null)
        {
            return NotFound();
        }

        // Check if course has enrollments
        var hasEnrollments = await _context.Enrollments.AnyAsync(e => e.CourseId == id);
        if (hasEnrollments)
        {
            TempData["ErrorMessage"] = "Không thể xóa khóa học đã có học viên đăng ký.";
            return RedirectToAction(nameof(Details), new { id });
        }

        _context.Courses.Remove(course);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Khóa học đã được xóa thành công.";
        return RedirectToAction(nameof(Index));
    }
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var viewModel = new CourseCreateEditViewModel
        {
            Categories = await _context.Categories
                .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
                .ToListAsync(),
            Instructors = await _userManager.GetUsersInRoleAsync(Constants.INSTRUCTOR_ROLE)
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CourseCreateEditViewModel model, IFormFile? thumbnail)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var course = new Course
                {
                    Title = model.Title,
                    ShortDescription = model.ShortDescription,
                    Description = model.Description,
                    Price = model.Price,
                    IsFree = model.IsFree,
                    Level = model.Level,
                    EstimatedDuration = model.EstimatedDuration,
                    CategoryId = model.CategoryId,
                    InstructorId = model.InstructorId,
                    Requirements = model.Requirements,
                    WhatYouWillLearn = model.WhatYouWillLearn,
                    IsPublished = false,
                    CreatedDate = DateTime.UtcNow
                };

                // Handle thumbnail upload
                if (thumbnail != null && thumbnail.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "courses");
                    Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(thumbnail.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await thumbnail.CopyToAsync(fileStream);
                    }

                    course.Thumbnail = "/uploads/courses/" + uniqueFileName;
                }

                _context.Courses.Add(course);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Khóa học đã được tạo thành công!";
                return RedirectToAction(nameof(Details), new { id = course.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating course");
                ModelState.AddModelError("", "Có lỗi xảy ra khi tạo khóa học.");
            }
        }

        // Reload data if validation fails
        model.Categories = await _context.Categories
            .Where(c => c.IsActive)
        .OrderBy(c => c.Name)
            .ToListAsync();
        model.Instructors = await _userManager.GetUsersInRoleAsync(Constants.INSTRUCTOR_ROLE);

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course == null)
        {
            return NotFound();
        }

        var viewModel = new CourseCreateEditViewModel
        {
            Id = course.Id,
            Title = course.Title,
            ShortDescription = course.ShortDescription,
            Description = course.Description,
            Price = course.Price,
            IsFree = course.IsFree,
            Level = course.Level,
            EstimatedDuration = course.EstimatedDuration,
            CategoryId = course.CategoryId,
            InstructorId = course.InstructorId,
            Requirements = course.Requirements,
            WhatYouWillLearn = course.WhatYouWillLearn,
            CurrentThumbnail = course.Thumbnail,
            Categories = await _context.Categories
                .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
                .ToListAsync(),
            Instructors = await _userManager.GetUsersInRoleAsync(Constants.INSTRUCTOR_ROLE)
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CourseCreateEditViewModel model, IFormFile? thumbnail)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var course = await _context.Courses.FindAsync(model.Id);
                if (course == null)
                {
                    return NotFound();
                }

                course.Title = model.Title;
                course.ShortDescription = model.ShortDescription;
                course.Description = model.Description;
                course.Price = model.Price;
                course.IsFree = model.IsFree;
                course.Level = model.Level;
                course.EstimatedDuration = model.EstimatedDuration;
                course.CategoryId = model.CategoryId;
                course.InstructorId = model.InstructorId;
                course.Requirements = model.Requirements;
                course.WhatYouWillLearn = model.WhatYouWillLearn;
                course.UpdatedDate = DateTime.UtcNow;

                // Handle new thumbnail upload
                if (thumbnail != null && thumbnail.Length > 0)
                {
                    // Delete old thumbnail if exists
                    if (!string.IsNullOrEmpty(course.Thumbnail))
                    {
                        var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", course.Thumbnail.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "courses");
                    Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(thumbnail.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await thumbnail.CopyToAsync(fileStream);
                    }

                    course.Thumbnail = "/uploads/courses/" + uniqueFileName;
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Khóa học đã được cập nhật thành công!";
                return RedirectToAction(nameof(Details), new { id = course.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating course");
                ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật khóa học.");
            }
        }

        // Reload data if validation fails
        model.Categories = await _context.Categories
            .Where(c => c.IsActive)
        .OrderBy(c => c.Name)
            .ToListAsync();
        model.Instructors = await _userManager.GetUsersInRoleAsync(Constants.INSTRUCTOR_ROLE);

        return View(model);
    }
}