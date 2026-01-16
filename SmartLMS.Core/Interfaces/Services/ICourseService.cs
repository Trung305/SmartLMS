using SmartLMS.Core.Common;
using SmartLMS.Core.DTOs;
using SmartLMS.Core.Entities;
using SmartLMS.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLMS.Core.Interfaces.Services
{
    public interface ICourseService
    {
        public Task<Course?> GetByIdAsync(Guid id);
        Task<PagedResult<CourseDto>> GetCoursesAsync(Guid userId, string role, string? search, CourseStatus? status, int? categoryId, int skip = 0, int take = 200, bool loadAll = false);
        public Task<List<Category>> GetCategoriesAsync();
        public Task<Course> CreateAsync(Course course);
        public Task<Course> UpdateAsync(Course course);
        public Task<Course> UpsertAsync(Course course);
        public Task<Result> DeleteAsync(Guid id);
    }
}
