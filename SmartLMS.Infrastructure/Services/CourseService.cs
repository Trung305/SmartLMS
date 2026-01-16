 using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartLMS.Core.Common;
using SmartLMS.Core.DTOs;
using SmartLMS.Core.Entities;
using SmartLMS.Core.Enums;
using SmartLMS.Core.Interfaces.Services;
using SmartLMS.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLMS.Infrastructure.Services
{
    public class CourseService : ICourseService
    {
        private readonly ApplicationDbContext _context;
        public CourseService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<PagedResult<CourseDto>> GetCoursesAsync(Guid userId, string role, string? search, CourseStatus? status, int? categoryId, int skip = 0, int take = 200, bool loadAll = false)
        {
            try
            {
                var query = _context.Courses
                    .Include(c => c.Category)
                    .Include(c => c.Enrollments)
                    .Include(c => c.Instructor)
                    .AsQueryable();

                if (role == "Instructor")
                {
                    query = query.Where(c => c.InstructorId == userId);
                }
                else if (role == "Student")
                {
                    query = query.Where(c => c.Enrollments.Any(e => e.StudentId == userId));
                }

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(c => c.Title.Contains(search));
                }

                if (status.HasValue)
                {
                    query = query.Where(c => c.Status == status.Value);
                }

                if (categoryId.HasValue)
                {
                    query = query.Where(c => c.CategoryId == categoryId.Value);
                }

                var totalCount = await query.CountAsync();

                var items = await query
                    .OrderByDescending(c => c.CreatedAt)
                    .Skip(skip)
                    .Take(take)
                    .Select(c => new CourseDto
                    {
                        Id = c.Id,
                        Title = c.Title,
                        EnrolledCount = c.Enrollments.Count,
                        CategoryName = c.Category != null ? c.Category.Name : "Chưa phân loại",
                        InstructorName = c.Instructor.FullName,
                        CreatedDate = c.CreatedAt,
                        Status = (int)c.Status,
                        IsFree = c.IsFree,
                        Thumbnail = c.Thumbnail,
                        Price = c.Price,
                        Level = (int)c.Level
                    })
                    .ToListAsync();

                return new PagedResult<CourseDto>
                {
                    Items = items,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi: " + ex.ToString());
                return new PagedResult<CourseDto>
                {
                    Items = new List<CourseDto>(),
                    TotalCount = 0,
                };
            }
        }
        public async Task<Course?> GetByIdAsync(Guid id)
        {
            return await _context.Courses
                .Include(c => c.Category)
                .FirstOrDefaultAsync(c => c.Id == id);
        }
        public async Task<Result> DeleteAsync(Guid id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
                return Result.Fail("Khóa học không tồn tại.");

            if (course.Status != CourseStatus.Draft || course.Status != CourseStatus.Pending)
                return Result.Fail("Không thể xóa khóa học khi đã công khai hoặc lưu trữ.");

            course.Status = CourseStatus.Remove;
            await _context.SaveChangesAsync();
            return Result.Success();
        }
        public async Task<List<Category>> GetCategoriesAsync()
        {
            return await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();
        }
        public async Task<Course> UpsertAsync(Course course)
        {
            if (course.Id == Guid.Empty)
            {
                return await CreateAsync(course);
            }
            else
            {
                return await UpdateAsync(course);
            }
        }
        public async Task<Course> CreateAsync(Course course)
        {
            course.Id = Guid.NewGuid();
            course.CreatedAt = DateTime.UtcNow;
            course.Status = CourseStatus.Pending;
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            return course;
        }

        public async Task<Course> UpdateAsync(Course course)
        {
            var existingCourse = await _context.Courses.FindAsync(course.Id);
            if (existingCourse == null)
            {
                throw new KeyNotFoundException($"Course with ID {course.Id} not found");
            }

            existingCourse.Title = course.Title;
            existingCourse.ShortDescription = course.ShortDescription;
            existingCourse.Description = course.Description;
            existingCourse.WhatYouWillLearn = course.WhatYouWillLearn;
            existingCourse.Requirements = course.Requirements;
            existingCourse.CategoryId = course.CategoryId;
            existingCourse.Level = course.Level;
            existingCourse.IsFree = course.IsFree;
            existingCourse.Price = course.Price;
            existingCourse.Thumbnail = course.Thumbnail;
            existingCourse.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return existingCourse;
        }
    }
}
