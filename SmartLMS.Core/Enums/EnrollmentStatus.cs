using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLMS.Core.Enums
{
    public enum EnrollmentStatus
    {
        Active = 1,     // Đang học
        Completed = 2,  // Đã hoàn thành
        Dropped = 3,    // Đã bỏ học
        Suspended = 4   // Bị đình chỉ
    }
}
