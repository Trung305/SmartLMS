using System.ComponentModel.DataAnnotations;
using SmartLMS.Core.Entities;

namespace SmartLMS.Web.Areas.Admin.Models;

public class LessonCreateEditViewModel
{
    public Guid Id { get; set; }

    [Required]
    public Guid CourseId { get; set; }

    public string CourseName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tiêu đề bài học là bắt buộc")]
    [StringLength(200, ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự")]
    [Display(Name = "Tiêu đề bài học")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nội dung bài học là bắt buộc")]
    [Display(Name = "Nội dung bài học")]
    public string Content { get; set; } = string.Empty;

    [Required(ErrorMessage = "Thời lượng là bắt buộc")]
    [Range(1, 36000, ErrorMessage = "Thời lượng phải từ 1 đến 36000 giây")]
    [Display(Name = "Thời lượng (giây)")]
    public int Duration { get; set; }

    [Required(ErrorMessage = "Thứ tự là bắt buộc")]
    [Range(1, 1000, ErrorMessage = "Thứ tự phải từ 1 đến 1000")]
    [Display(Name = "Thứ tự bài học")]
    public int OrderIndex { get; set; }

    [Display(Name = "Cho phép xem trước")]
    public bool IsPreview { get; set; }

    [Display(Name = "Xuất bản")]
    public bool IsPublished { get; set; } = true;

    public string? CurrentVideoUrl { get; set; }

}
public class LessonIndexViewModel
{
    public Course Course { get; set; } = new();
    public IList<Lesson> Lessons { get; set; } = new List<Lesson>();
}