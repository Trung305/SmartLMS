using SmartLMS.Core.Entities;

namespace SmartLMS.Web.Models.ViewModels;

public class CourseDetailsViewModel
{
    public Course Course { get; set; } = new();
    public bool IsEnrolled { get; set; }
    public IList<Lesson> Lessons { get; set; } = new List<Lesson>();
}