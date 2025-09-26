using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLMS.Core.Common
{
    public static class Constants
    {
        // Roles
        public const string ADMIN_ROLE = "Admin";
        public const string INSTRUCTOR_ROLE = "Instructor";
        public const string STUDENT_ROLE = "Student";

        // File paths
        public const string UPLOAD_PATH = "uploads";
        public const string COURSE_IMAGES_PATH = "uploads/courses";
        public const string VIDEOS_PATH = "uploads/videos";
        public const string AVATARS_PATH = "uploads/avatars";

        // Validation
        public const int MAX_FILE_SIZE = 100 * 1024 * 1024; // 100MB
        public const int MIN_PASSWORD_LENGTH = 6;
        public const int MAX_COURSE_TITLE_LENGTH = 200;

        // Pagination
        public const int DEFAULT_PAGE_SIZE = 12;
        public const int MAX_PAGE_SIZE = 100;

        // Course settings
        public const decimal FREE_COURSE_PRICE = 0;
        public const int MIN_PASSING_SCORE = 70;
    }
}
