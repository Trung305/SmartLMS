using SmartLMS.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLMS.Core.Interfaces.Services
{
    public interface IEnrollmentService
    {
        Task<(bool success, string message)> EnrollAsync(Guid userId, Guid courseId);
        Task<bool> IsEnrolledAsync(Guid userId, Guid courseId);
        Task<IEnumerable<Enrollment>> GetUserEnrollmentsAsync(Guid userId);
        Task<Enrollment> GetEnrollmentAsync(Guid userId, Guid courseId);
        Task<bool> UpdateProgressAsync(Guid enrollmentId, decimal progress);
        Task<int> GetEnrolledCountAsync(Guid courseId);
    }
}
