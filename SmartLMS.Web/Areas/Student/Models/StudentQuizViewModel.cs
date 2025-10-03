using SmartLMS.Core.Entities;

namespace SmartLMS.Web.Areas.Student.Models;

public class StudentQuizViewModel
{
    public Quiz Quiz { get; set; } = new();
    public QuizAttempt? CurrentAttempt { get; set; }
    public IList<QuestionViewModel> Questions { get; set; } = new List<QuestionViewModel>();
    public QuizSessionInfo SessionInfo { get; set; } = new();
    public IList<QuizAttempt> PreviousAttempts { get; set; } = new List<QuizAttempt>();
}

public class QuestionViewModel
{
    public Question Question { get; set; } = new();
    public IList<Answer> Answers { get; set; } = new List<Answer>();
    public string? StudentAnswer { get; set; }
    public bool IsAnswered { get; set; }
    public int QuestionNumber { get; set; }
}

public class QuizSessionInfo
{
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public TimeSpan TimeLimit { get; set; }
    public TimeSpan TimeRemaining { get; set; }
    public bool IsTimeUp { get; set; }
    public int AttemptNumber { get; set; }
    public int MaxAttempts { get; set; }
    public int RemainingAttempts { get; set; }
    public decimal PassingScore { get; set; }
    public bool CanRetake { get; set; }
}

public class QuizResultViewModel
{
    public Quiz Quiz { get; set; } = new();
    public QuizAttempt Attempt { get; set; } = new();
    public IList<QuestionResultDetail> QuestionResults { get; set; } = new List<QuestionResultDetail>();
    public QuizPerformanceAnalysis Analysis { get; set; } = new();
    public bool CanRetake { get; set; }
    public DateTime? NextRetakeDate { get; set; }
}

public class QuestionResultDetail
{
    public Question Question { get; set; } = new();
    public string StudentAnswer { get; set; } = string.Empty;
    public string CorrectAnswer { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int Points { get; set; }
    public int MaxPoints { get; set; }
    public string? Explanation { get; set; }
}

public class QuizPerformanceAnalysis
{
    public int TotalQuestions { get; set; }
    public int CorrectAnswers { get; set; }
    public int IncorrectAnswers { get; set; }
    public decimal AccuracyPercentage { get; set; }
    public decimal ScorePercentage { get; set; }
    public bool IsPassed { get; set; }
    public TimeSpan TimeTaken { get; set; }
    public string PerformanceLevel { get; set; } = string.Empty; // Excellent, Good, Fair, Poor
    public IList<string> Strengths { get; set; } = new List<string>();
    public IList<string> ImprovementAreas { get; set; } = new List<string>();
}