using SmartLMS.Core.Common;
using SmartLMS.Core.Entities;

namespace SmartLMS.Web.Areas.Admin.Models;

public class AdminUsersViewModel
{
    public PagedResult<UserWithRoleModel> Users { get; set; } = new();
    public string? SearchQuery { get; set; }
    public string? SelectedRole { get; set; }
    public string? SelectedStatus { get; set; }
    public IList<string> AvailableRoles { get; set; } = new List<string>();
}

public class UserWithRoleModel
{
    public ApplicationUser User { get; set; } = new();
    public IList<string> Roles { get; set; } = new List<string>();
    public UserStatistics? Statistics { get; set; }
}

public class UserStatistics
{
    public int TotalEnrollments { get; set; }
    public int CompletedCourses { get; set; }
    public int CreatedCourses { get; set; } // For Instructors
    public decimal TotalRevenue { get; set; } // For Instructors
    public DateTime? LastLoginDate { get; set; }
    public DateTime? LastActivityDate { get; set; }
    public int TotalLearningTime { get; set; } // in minutes
}

public class AdminUserDetailsViewModel
{
    public UserWithRoleModel UserWithRoles { get; set; } = new();
    public IList<Enrollment>? RecentEnrollments { get; set; } // For Students
    public IList<Course>? CreatedCourses { get; set; } // For Instructors
    public IList<string> AvailableRoles { get; set; } = new List<string>();
}
