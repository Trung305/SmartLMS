using SmartLMS.Core.Entities;

namespace SmartLMS.Web.Areas.Instructor.Models
{
    public class InstructorDashboardViewModel
    {
        public ApplicationUser Instructor { get; set; } = new();
    }
}
