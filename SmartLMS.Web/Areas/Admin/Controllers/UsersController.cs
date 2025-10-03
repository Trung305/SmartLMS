
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
public class UsersController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly ILogger<UsersController> _logger;

    public UsersController(ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        ILogger<UsersController> logger)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task<IActionResult> Index(int page = 1, string? search = null, string? role = null)
    {
        var pageSize = 20;
        var query = _userManager.Users.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(u => u.FirstName.Contains(search) ||
                                   u.LastName.Contains(search) ||
                                   u.Email.Contains(search));
        }

        var totalCount = await query.CountAsync();
        var users = await query
            .OrderByDescending(u => u.CreatedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Get roles for each user
        var usersWithRoles = new List<UserWithRoleModel>();
        foreach (var user in users)
        {
            var userRoles = await _userManager.GetRolesAsync(user);
            usersWithRoles.Add(new UserWithRoleModel
            {
                User = user,
                Roles = userRoles
            });
        }

        var viewModel = new AdminUsersViewModel
        {
            Users = new PagedResult<UserWithRoleModel>
            {
                Items = usersWithRoles,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            },
            SearchQuery = search,
            SelectedRole = role,
            AvailableRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync()
        };

        return View(viewModel);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            return NotFound();
        }

        var userRoles = await _userManager.GetRolesAsync(user);
        var viewModel = new AdminUserDetailsViewModel
        {
            UserWithRoles = new UserWithRoleModel
            {
                User = user,
                Roles = userRoles
            },
            RecentEnrollments = await _context.Enrollments
            .Where(e => e.StudentId == user.Id)
            .OrderByDescending(e => e.EnrollmentDate)
            .Take(5)
            .Include(e => e.Course)
                .ThenInclude(c => c.Category)
            .ToListAsync(),

            CreatedCourses = await _context.Courses
            .Where(c => c.InstructorId == user.Id)
            .ToListAsync(),

            AvailableRoles = new List<string> { "Admin", "Instructor", "Student" } // ví dụ, bạn có thể lấy từ RoleManager
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> ToggleActive(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            return NotFound();
        }

        user.IsActive = !user.IsActive;
        await _userManager.UpdateAsync(user);

        TempData["SuccessMessage"] = user.IsActive ?
            "Tài khoản đã được kích hoạt." :
            "Tài khoản đã bị vô hiệu hóa.";

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> ChangeRole(Guid id, string newRole)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            return NotFound();
        }

        // Remove all current roles
        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);

        // Add new role
        if (!string.IsNullOrEmpty(newRole))
        {
            await _userManager.AddToRoleAsync(user, newRole);
        }

        TempData["SuccessMessage"] = "Vai trò người dùng đã được cập nhật.";
        return RedirectToAction(nameof(Details), new { id });
    }
    [HttpGet]
    [HttpGet]
    public IActionResult Create()
    {
        var viewModel = new UserCreateEditViewModel
        {
            AvailableRoles = _roleManager.Roles.Select(r => r.Name!).ToList()
        };

        return View(viewModel);
    }

    // POST: Admin/Users/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserCreateEditViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(model.SelectedRole))
                    {
                        await _userManager.AddToRoleAsync(user, model.SelectedRole);
                    }

                    TempData["SuccessMessage"] = "Người dùng đã được tạo thành công!";
                    return RedirectToAction(nameof(Details), new { id = user.Id });
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                ModelState.AddModelError("", "Có lỗi xảy ra khi tạo người dùng.");
            }
        }

        model.AvailableRoles = _roleManager.Roles.Select(r => r.Name!).ToList();
        return View(model);
    }

    // GET: Admin/Users/Edit/5
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            return NotFound();
        }

        var userRoles = await _userManager.GetRolesAsync(user);

        var viewModel = new UserCreateEditViewModel
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            IsActive = user.IsActive,
            SelectedRole = userRoles.FirstOrDefault() ?? "",
            AvailableRoles = _roleManager.Roles.Select(r => r.Name!).ToList()
        };

        return View(viewModel);
    }

    // POST: Admin/Users/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UserCreateEditViewModel model)
    {
        ModelState.Remove(nameof(model.Password));
        ModelState.Remove(nameof(model.ConfirmPassword));

        if (ModelState.IsValid)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(model.Id.ToString());
                if (user == null)
                {
                    return NotFound();
                }

                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Email = model.Email;
                user.UserName = model.Email;
                user.IsActive = model.IsActive;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    // Update role
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);

                    if (!string.IsNullOrEmpty(model.SelectedRole))
                    {
                        await _userManager.AddToRoleAsync(user, model.SelectedRole);
                    }

                    TempData["SuccessMessage"] = "Người dùng đã được cập nhật thành công!";
                    return RedirectToAction(nameof(Details), new { id = user.Id });
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user");
                ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật người dùng.");
            }
        }

        model.AvailableRoles = _roleManager.Roles.Select(r => r.Name!).ToList();
        return View(model);
    }

}