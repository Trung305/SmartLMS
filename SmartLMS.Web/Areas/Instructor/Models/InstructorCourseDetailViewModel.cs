using SmartLMS.Core.Entities;

namespace SmartLMS.Web.Areas.Instructor.Models
{
    public class InstructorCourseDetailViewModel
    {
        public ApplicationUser Instructor { get; set; } = new();
    }
}
