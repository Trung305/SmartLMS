using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLMS.Core.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Icon { get; set; }        // Icon cho danh mục
        public int? ParentCategoryId { get; set; } // Danh mục cha (cho danh mục con)
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Category? ParentCategory { get; set; }                    // Danh mục cha
        public ICollection<Category> SubCategories { get; set; } = new List<Category>(); // Danh mục con
        public ICollection<Course> Courses { get; set; } = new List<Course>();          // Khóa học trong danh mục
    }
}
