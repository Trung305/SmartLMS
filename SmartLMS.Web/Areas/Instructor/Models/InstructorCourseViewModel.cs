using SmartLMS.Core.Common;
using SmartLMS.Core.DTOs;
using SmartLMS.Core.Entities;

namespace SmartLMS.Web.Areas.Instructor.Models
{
    public class InstructorCourseViewModel
    {
        public PagedResult<CourseDto> Courses { get; set; } = new();
        public string? SearchQuery { get; set; }
        public int? SelectedCategory { get; set; }
        public int? SelectedStatus { get; set; }
        public IList<Category> Categories { get; set; } = new List<Category>();
    }
}
