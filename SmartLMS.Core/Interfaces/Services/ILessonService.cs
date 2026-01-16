using Microsoft.AspNetCore.Http;
using SmartLMS.Core.Common;
using SmartLMS.Core.DTOs;
using SmartLMS.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLMS.Core.Interfaces.Services
{
    public interface ILessonService
    {
        Task<Result<LessonIndexDto>> GetLessonsByCourseAsync(Guid courseId);

        Task<Result<LessonCreateEditDto>> GetCreateDtoAsync(Guid courseId);

        Task<Result<Guid>> CreateLessonAsync(LessonCreateEditDto dto, IFormFile? video);

        Task<Result<LessonCreateEditDto>> GetEditDtoAsync(Guid lessonId);

        Task<Result> UpdateLessonAsync(LessonCreateEditDto dto, IFormFile? video);

        Task<Result<Lesson>> GetLessonDetailsAsync(Guid lessonId);

        Task<Result> DeleteLessonAsync(Guid lessonId);

        Task<Result> TogglePublishAsync(Guid lessonId);

        Task<Result> ReorderLessonsAsync(Guid courseId, List<Guid> lessonIds);
    }
}
