using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartLMS.Core.Common;
using SmartLMS.Core.Entities;
using SmartLMS.Core.Enums;
using SmartLMS.Core.Interfaces.Services;
using SmartLMS.Infrastructure.Data;
using SmartLMS.Infrastructure.Services;
using SmartLMS.Web.Areas.Admin.Models;
using SmartLMS.Web.Areas.Instructor.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SmartLMS.Web.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    [Authorize(Roles = Constants.INSTRUCTOR_ROLE)]
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICourseService _courseService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CoursesController> _logger;
        private readonly IFileUploadService _fileUploadService;
        private readonly IWebHostEnvironment _env;

        public CoursesController(IWebHostEnvironment env, IFileUploadService fileUploadService, ICourseService courseService, ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<CoursesController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _courseService = courseService;
            _fileUploadService = fileUploadService;
            _env = env;
        }
        [HttpGet]
        public async Task<IActionResult> Index(string? searchQuery, int? selectedCategory, int? selectedStatus, int page = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }
            CourseStatus? courseStatus = selectedStatus.HasValue ? (CourseStatus)selectedStatus.Value : null;
            try
            {
                var vm = new InstructorCourseViewModel
                {
                    Courses = null,
                    SearchQuery = searchQuery,
                    SelectedStatus = selectedStatus,
                    SelectedCategory = selectedCategory,
                    Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync()
                };

                return View(vm);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetCourses(string searchQuery, int? selectedCategory,int? selectedStatus,int skip = 0,int take = 200,bool loadAll = false) 
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            CourseStatus? courseStatus = selectedStatus.HasValue ? (CourseStatus)selectedStatus.Value : null;

            try
            {
                var result = await _courseService.GetCoursesAsync(
                    userId: user.Id,
                    role: "Instructor",
                    search: searchQuery,
                    status: courseStatus,
                    categoryId: selectedCategory,
                    skip: skip,
                    take: take,
                    loadAll: loadAll);

                if (result == null || !result.Items.Any())
                {
                    return Ok(null);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.ToString());
                return Ok(null);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Upsert(Guid? id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }
       
            var model = new InsructorCourseUpsertViewModel
            {
                Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync()
            };


            if (id.HasValue && id.Value != Guid.Empty)
            {
                var course = await _courseService.GetByIdAsync(id.Value);
                if (course == null)
                {
                    return NotFound();
                }

                model.Id = course.Id;
                model.Title = course.Title;
                model.ShortDescription = course.ShortDescription;
                model.Description = course.Description;
                model.WhatYouWillLearn = course.WhatYouWillLearn;
                model.Requirements = course.Requirements;
                model.CategoryId = course.CategoryId;
                model.Level = course.Level;
                model.IsFree = course.IsFree;
                model.Price = course.Price;
                model.CurrentThumbnail = course.Thumbnail;
            }

            return PartialView("Upsert", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(InsructorCourseUpsertViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    string thumbnailUrl = model.CurrentThumbnail;
                    if (model.ThumbnailFile != null && model.ThumbnailFile.Length > 0)
                    {
                        Stream stream = model.ThumbnailFile.OpenReadStream();
                        thumbnailUrl = await _fileUploadService.SaveFileAsync(stream, model.ThumbnailFile.FileName);

                        if (!string.IsNullOrEmpty(model.CurrentThumbnail))
                        {
                            var fullPath = Path.Combine(_env.WebRootPath, model.CurrentThumbnail.TrimStart('/'));
                            _fileUploadService.DeleteFile(fullPath);
                        }
                    }

                    bool isEdit = model.Id != Guid.Empty;

                    if (isEdit)
                    {
                        await _courseService.UpdateAsync(new Course
                        {
                            Id = model.Id,
                            Title = model.Title,
                            ShortDescription = model.ShortDescription,
                            Description = model.Description,
                            WhatYouWillLearn = model.WhatYouWillLearn,
                            Requirements = model.Requirements,
                            CategoryId = model.CategoryId,
                            Level = model.Level,
                            IsFree = model.IsFree,
                            Price = model.Price,
                            Thumbnail = thumbnailUrl,
                            InstructorId = user.Id,
                            UpdatedAt = DateTime.Now,
                            UpdatedBy = user.Id,
                        });
                    }
                    else
                    {
                        await _courseService.CreateAsync(new Course
                        {
                            Title = model.Title,
                            ShortDescription = model.ShortDescription,
                            Description = model.Description,
                            WhatYouWillLearn = model.WhatYouWillLearn,
                            Requirements = model.Requirements,
                            CategoryId = model.CategoryId,
                            Level = model.Level,
                            IsFree = model.IsFree,
                            Price = model.Price,
                            Thumbnail = thumbnailUrl,
                            InstructorId = user.Id,
                            Status = model.IsDraft ? CourseStatus.Draft : CourseStatus.Pending,
                            CreatedAt = DateTime.Now,
                            CreatedBy = user.Id,
                        });
                    }

                    return Json(new
                    {
                        success = true,
                        message = isEdit ? "Cập nhật khóa học thành công!" : "Tạo khóa học thành công!",
                        redirectUrl = Url.Action("Index")
                    });
                }
                catch (Exception ex)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Có lỗi: " + ex.Message
                    });
                }
            }

            var errors = ModelState
               .Where(x => x.Value.Errors.Any())
               .ToDictionary(
                   x => x.Key,
                   x => x.Value.Errors.Select(e => e.ErrorMessage).FirstOrDefault()
               );

            return Json(new
            {
                success = false,
                message = "Vui lòng kiểm tra lại thông tin",
                errors = errors
            });
        }
        [HttpDelete]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _courseService.DeleteAsync(id);
                return Json(new
                {
                    success = result.IsSuccess,
                    message = result.IsSuccess ? "Xóa thành công." : result.ErrorMessage
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Lỗi hệ thống: " + ex.Message
                });
            }
        }
    }
}
