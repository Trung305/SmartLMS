using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLMS.Core.Enums
{
    public enum CourseStatus
    {
        Remove = -1,      // Xóa
        Draft = 1,      // Bản nháp
        Pending = 2,  // Chờ duyệt
        Published = 3,    // Công khai
        Archived = 4,   // Lưu trữ 
        Rejected = 5 // Từ chối
    }
}
