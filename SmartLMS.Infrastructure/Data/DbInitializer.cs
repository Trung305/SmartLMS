using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartLMS.Core.Common;
using SmartLMS.Core.Entities;
using SmartLMS.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLMS.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<Guid>> roleManager)
        {
            // Tạo database nếu chưa tồn tại
            await context.Database.EnsureCreatedAsync();

            // Seed roles nếu chưa có
            await SeedRolesAsync(roleManager);

            // Seed users nếu chưa có
            await SeedUsersAsync(userManager);

            // Seed sample courses
            await SeedCoursesAsync(context, userManager);

            await context.SaveChangesAsync();
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
        {
            var roles = new[] { Constants.ADMIN_ROLE, Constants.INSTRUCTOR_ROLE, Constants.STUDENT_ROLE };

            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole<Guid>
                    {
                        Id = Guid.NewGuid(),
                        Name = roleName,
                        NormalizedName = roleName.ToUpper()
                    });
                }
            }
        }

        private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager)
        {
            // Admin user
            if (await userManager.FindByEmailAsync("admin@smartlms.com") == null)
            {
                var admin = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    UserName = "admin@smartlms.com",
                    Email = "admin@smartlms.com",
                    FirstName = "Admin",
                    LastName = "System",
                    EmailConfirmed = true,
                    IsActive = true
                };

                var result = await userManager.CreateAsync(admin, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, Constants.ADMIN_ROLE);
                }
            }

            // Instructor user
            if (await userManager.FindByEmailAsync("instructor@smartlms.com") == null)
            {
                var instructor = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    UserName = "instructor@smartlms.com",
                    Email = "instructor@smartlms.com",
                    FirstName = "Nguyễn",
                    LastName = "Văn Giảng",
                    EmailConfirmed = true,
                    IsActive = true
                };

                var result = await userManager.CreateAsync(instructor, "Instructor123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(instructor, Constants.INSTRUCTOR_ROLE);
                }
            }

            // Student user
            if (await userManager.FindByEmailAsync("student@smartlms.com") == null)
            {
                var student = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    UserName = "student@smartlms.com",
                    Email = "student@smartlms.com",
                    FirstName = "Trần",
                    LastName = "Thị Học",
                    EmailConfirmed = true,
                    IsActive = true
                };

                var result = await userManager.CreateAsync(student, "Student123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(student, Constants.STUDENT_ROLE);
                }
            }
        }

        private static async Task SeedCoursesAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            if (await context.Courses.AnyAsync())
                return; // Đã có dữ liệu rồi

            var instructor = await userManager.FindByEmailAsync("instructor@smartlms.com");
            if (instructor == null) return;

            var sampleCourses = new[]
            {
            new Course
            {
                Id = Guid.NewGuid(),
                Title = "ASP.NET Core MVC cho người mới bắt đầu",
                ShortDescription = "Học cách xây dựng ứng dụng web với ASP.NET Core MVC",
                Description = "Khóa học chi tiết về ASP.NET Core MVC từ cơ bản đến nâng cao...",
                Price = 0,
                IsFree = true,
                Level = CourseLevel.Beginner,
                CategoryId = 1,
                InstructorId = instructor.Id,
                IsPublished = true,
                PublishedDate = DateTime.UtcNow,
                EstimatedDuration = 720 // 12 hours
            },
            new Course
            {
                Id = Guid.NewGuid(),
                Title = "JavaScript ES6+ Nâng cao",
                ShortDescription = "Nắm vững JavaScript hiện đại với ES6+",
                Description = "Khóa học về các tính năng mới của JavaScript...",
                Price = 299000,
                IsFree = false,
                Level = CourseLevel.Intermediate,
                CategoryId = 1,
                InstructorId = instructor.Id,
                IsPublished = true,
                PublishedDate = DateTime.UtcNow,
                EstimatedDuration = 600 // 10 hours
            }
        };

            context.Courses.AddRange(sampleCourses);
        }
    }
}
