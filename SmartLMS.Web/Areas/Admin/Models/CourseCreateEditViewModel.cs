using System.ComponentModel.DataAnnotations;
using SmartLMS.Core.Entities;
using SmartLMS.Core.Enums;

namespace SmartLMS.Web.Areas.Admin.Models;

public class CourseCreateEditViewModel
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Tiêu đề khóa học là bắt buộc")]
    [StringLength(200, ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự")]
    [Display(Name = "Tiêu đề khóa học")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mô tả ngắn là bắt buộc")]
    [StringLength(500, ErrorMessage = "Mô tả ngắn không được vượt quá 500 ký tự")]
    [Display(Name = "Mô tả ngắn")]
    public string ShortDescription { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mô tả chi tiết là bắt buộc")]
    [Display(Name = "Mô tả chi tiết")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Giá là bắt buộc")]
    [Range(0, 999999999, ErrorMessage = "Giá phải từ 0 đến 999,999,999")]
    [Display(Name = "Giá (VNĐ)")]
    public decimal Price { get; set; }

    [Display(Name = "Miễn phí")]
    public bool IsFree { get; set; }

    [Required(ErrorMessage = "Cấp độ là bắt buộc")]
    [Display(Name = "Cấp độ")]
    public CourseLevel Level { get; set; } = CourseLevel.Beginner;

    [Required(ErrorMessage = "Thời lượng ước tính là bắt buộc")]
    [Range(1, 10000, ErrorMessage = "Thời lượng phải từ 1 đến 10,000 phút")]
    [Display(Name = "Thời lượng ước tính (phút)")]
    public int EstimatedDuration { get; set; }

    [Required(ErrorMessage = "Danh mục là bắt buộc")]
    [Display(Name = "Danh mục")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Giảng viên là bắt buộc")]
    [Display(Name = "Giảng viên")]
    public Guid InstructorId { get; set; }

    [Display(Name = "Yêu cầu")]
    public string? Requirements { get; set; }

    [Display(Name = "Bạn sẽ học được gì")]
    public string? WhatYouWillLearn { get; set; }

    public string? CurrentThumbnail { get; set; }

    // For dropdowns
    public IList<Category> Categories { get; set; } = new List<Category>();
    public IList<ApplicationUser> Instructors { get; set; } = new List<ApplicationUser>();
}