using SmartLMS.Core.Entities;
using SmartLMS.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace SmartLMS.Web.Areas.Instructor.Models
{
    public class InsructorCourseUpsertViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Tiêu đề khóa học là bắt buộc")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mô tả ngắn là bắt buộc")]
        [StringLength(500)]
        public string ShortDescription { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mô tả chi tiết là bắt buộc")]
        public string Description { get; set; } = string.Empty;

        public bool IsFree { get; set; } = false;

        [Range(0, 999999999, ErrorMessage = "Giá không hợp lệ")]
        public decimal Price { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn danh mục")]
        public int CategoryId { get; set; }

        public CourseLevel Level { get; set; } = CourseLevel.Beginner;

        public string? Requirements { get; set; }

        public string? WhatYouWillLearn { get; set; }
        public IFormFile? ThumbnailFile { get; set; }
        public string? CurrentThumbnail { get; set; }
        public bool IsDraft { get; set; } = false;

        public IList<Category> Categories { get; set; } = new List<Category>();
    }
}
