using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartLMS.Core.Entities;
using SmartLMS.Infrastructure.Data.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLMS.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // DbSets - đại diện cho các bảng trong database
        public DbSet<Course> Courses { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<LessonProgress> LessonProgress { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<QuizAttempt> QuizAttempts { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Áp dụng tất cả configurations
            builder.ApplyConfiguration(new UserConfiguration());
            builder.ApplyConfiguration(new CategoryConfiguration());
            builder.ApplyConfiguration(new CourseConfiguration());
            builder.ApplyConfiguration(new EnrollmentConfiguration());

            // Cấu hình các bảng còn lại trực tiếp
            ConfigureLessonEntity(builder);
            ConfigureLessonProgressEntity(builder);
            ConfigureQuizEntities(builder);

            // Seed data ban đầu
            SeedDefaultData(builder);
        }

        private void ConfigureLessonEntity(ModelBuilder builder)
        {
            builder.Entity<Lesson>(entity =>
            {
                entity.ToTable("Lessons");

                entity.Property(l => l.Title)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(l => l.Content)
                    .HasColumnType("NVARCHAR(MAX)");

                entity.Property(l => l.VideoUrl)
                    .HasMaxLength(500);

                entity.Property(l => l.CreatedDate)
                    .HasDefaultValueSql("GETUTCDATE()");

                // Index for ordering lessons in course
                entity.HasIndex(l => new { l.CourseId, l.OrderIndex })
                    .HasDatabaseName("IX_Lessons_Course_Order");
            });
        }

        private void ConfigureLessonProgressEntity(ModelBuilder builder)
        {
            builder.Entity<LessonProgress>(entity =>
            {
                entity.ToTable("LessonProgress");

                entity.Property(lp => lp.LastAccessDate)
                    .HasDefaultValueSql("GETUTCDATE()");

                // Unique constraint: 1 enrollment chỉ có 1 progress per lesson
                entity.HasIndex(lp => new { lp.EnrollmentId, lp.LessonId })
                    .IsUnique()
                    .HasDatabaseName("IX_LessonProgress_Enrollment_Lesson_Unique");

                // Index for completion queries
                entity.HasIndex(lp => new { lp.EnrollmentId, lp.IsCompleted })
                    .HasDatabaseName("IX_LessonProgress_Completion");

                // QUAN TRỌNG: Đây là nơi fix lỗi
                entity.HasOne(lp => lp.Enrollment)
                    .WithMany(e => e.LessonProgress)
                    .HasForeignKey(lp => lp.EnrollmentId)
                    .OnDelete(DeleteBehavior.Cascade); // CASCADE cho Enrollment

                entity.HasOne(lp => lp.Lesson)
                    .WithMany(l => l.LessonProgress)
                    .HasForeignKey(lp => lp.LessonId)
                    .OnDelete(DeleteBehavior.Restrict); // RESTRICT cho Lesson ← FIX TẠI ĐÂY
            });
        }

        private void ConfigureQuizEntities(ModelBuilder builder)
        {
            // Quiz configuration
            builder.Entity<Quiz>(entity =>
            {
                entity.ToTable("Quizzes");

                entity.Property(q => q.Title)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(q => q.PassingScore)
                    .HasColumnType("decimal(5,2)");

                entity.HasMany(q => q.Questions)
                    .WithOne(qu => qu.Quiz)
                    .HasForeignKey(qu => qu.QuizId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Question configuration
            builder.Entity<Question>(entity =>
            {
                entity.ToTable("Questions");

                entity.Property(q => q.Content)
                    .HasColumnType("NVARCHAR(MAX)")
                    .IsRequired();

                entity.Property(q => q.Type)
                    .HasConversion<int>();

                entity.HasIndex(q => new { q.QuizId, q.OrderIndex })
                    .HasDatabaseName("IX_Questions_Quiz_Order");

                entity.HasMany(q => q.Answers)
                    .WithOne(a => a.Question)
                    .HasForeignKey(a => a.QuestionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Answer configuration
            builder.Entity<Answer>(entity =>
            {
                entity.ToTable("Answers");

                entity.Property(a => a.Content)
                    .HasMaxLength(500)
                    .IsRequired();

                entity.HasIndex(a => new { a.QuestionId, a.OrderIndex })
                    .HasDatabaseName("IX_Answers_Question_Order");
            });

            // QuizAttempt configuration
            builder.Entity<QuizAttempt>(entity =>
            {
                entity.ToTable("QuizAttempts");

                entity.Property(qa => qa.Score)
                    .HasColumnType("decimal(5,2)");

                entity.Property(qa => qa.MaxScore)
                    .HasColumnType("decimal(5,2)");

                entity.Property(qa => qa.StudentAnswers)
                    .HasColumnType("NVARCHAR(MAX)");

                entity.Property(qa => qa.StartedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.HasIndex(qa => new { qa.StudentId, qa.QuizId, qa.AttemptNumber })
                    .HasDatabaseName("IX_QuizAttempts_Student_Quiz_Attempt");
            });
        }

        private void SeedDefaultData(ModelBuilder builder)
        {
            // Seed Roles
            var roles = new[]
            {
            new IdentityRole<Guid> { Id = Guid.NewGuid(), Name = "Admin", NormalizedName = "ADMIN" },
            new IdentityRole<Guid> { Id = Guid.NewGuid(), Name = "Instructor", NormalizedName = "INSTRUCTOR" },
            new IdentityRole<Guid> { Id = Guid.NewGuid(), Name = "Student", NormalizedName = "STUDENT" }
        };
            builder.Entity<IdentityRole<Guid>>().HasData(roles);

            // Seed Categories
            var categories = new[]
            {
            new Category { Id = 1, Name = "Lập trình", Description = "Các khóa học về lập trình", Icon = "fa-code" },
            new Category { Id = 2, Name = "Thiết kế", Description = "Các khóa học về thiết kế", Icon = "fa-paint-brush" },
            new Category { Id = 3, Name = "Marketing", Description = "Các khóa học về marketing", Icon = "fa-bullhorn" },
            new Category { Id = 4, Name = "Kinh doanh", Description = "Các khóa học về kinh doanh", Icon = "fa-briefcase" }
        };
            builder.Entity<Category>().HasData(categories);
        }
    }
}
