using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLMS.Core.DTOs
{
    public class CourseDto
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public int EnrolledCount { get; set; } 

        public string CategoryName { get; set; } = string.Empty;

        public string InstructorName { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; }

        public int Status { get; set; }
        public bool IsFree { get; set; }
        public string? Thumbnail { get; set; }
        public decimal Price { get; set; }
        public int Level { get; set; }
    }
}
