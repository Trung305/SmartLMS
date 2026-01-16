using SmartLMS.Core.Common;
using SmartLMS.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLMS.Core.Interfaces.Services
{
    public interface IQuizzesService
    {
        Task<Result<QuizDetailDto>> GetQuizByIdAsync(Guid quizId);
        Task<Result<List<QuizDto>>> GetQuizzesByCourseAsync(Guid courseId);
        Task<Result<CreateEditQuizDto>> GetCreateViewModelAsync(Guid courseId, Guid? lessonId = null);
        Task<Result<CreateEditQuizDto>> GetEditViewModelAsync(Guid quizId);
        Task<Result<Guid>> CreateQuizAsync(CreateEditQuizDto model);
        Task<Result<Guid>> UpdateQuizAsync(CreateEditQuizDto model);
        Task<Result<bool>> DeleteQuizAsync(Guid quizId);

        Task<Result<bool>> ValidateQuizDataAsync(CreateEditQuizDto model);
    }
}
