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

            builder.ApplyConfiguration(new UserConfiguration());
            builder.ApplyConfiguration(new CategoryConfiguration());
            builder.ApplyConfiguration(new CourseConfiguration());
            builder.ApplyConfiguration(new EnrollmentConfiguration());

            ConfigureLessonEntity(builder);
            ConfigureLessonProgressEntity(builder);
            ConfigureQuizEntities(builder);

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

                entity.HasIndex(lp => new { lp.EnrollmentId, lp.LessonId })
                    .IsUnique()
                    .HasDatabaseName("IX_LessonProgress_Enrollment_Lesson_Unique");

                entity.HasIndex(lp => new { lp.EnrollmentId, lp.IsCompleted })
                    .HasDatabaseName("IX_LessonProgress_Completion");

                entity.HasOne(lp => lp.Enrollment)
                    .WithMany(e => e.LessonProgress)
                    .HasForeignKey(lp => lp.EnrollmentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(lp => lp.Lesson)
                    .WithMany(l => l.LessonProgress)
                    .HasForeignKey(lp => lp.LessonId)
                    .OnDelete(DeleteBehavior.Restrict); 
            });
        }

        private void ConfigureQuizEntities(ModelBuilder builder)
        {
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

            builder.Entity<Answer>(entity =>
            {
                entity.ToTable("Answers");

                entity.Property(a => a.Content)
                    .HasMaxLength(500)
                    .IsRequired();

                entity.HasIndex(a => new { a.QuestionId, a.OrderIndex })
                    .HasDatabaseName("IX_Answers_Question_Order");
            });

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
            var roles = new[]
            {
            new IdentityRole<Guid> { Id = new Guid("963E29CC-209B-4696-8BB1-F7631F74901E"), Name = "Admin", NormalizedName = "ADMIN", ConcurrencyStamp = "1"  },
            new IdentityRole<Guid> { Id = new Guid("1D0FE904-5392-4F48-AD47-45226F1B319D"), Name = "Instructor", NormalizedName = "INSTRUCTOR", ConcurrencyStamp = "2" },
            new IdentityRole<Guid> { Id = new Guid("6D9C1299-7E7B-4689-848A-58A0623950F5"), Name = "Student", NormalizedName = "STUDENT", ConcurrencyStamp = "3" }
        };
            builder.Entity<IdentityRole<Guid>>().HasData(roles);

            var categories = new[]
            {
            new Category { Id = 1, Name = "Lập trình", Description = "Các khóa học về lập trình", Icon = "fa-code" , CreatedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Category { Id = 2, Name = "Thiết kế", Description = "Các khóa học về thiết kế", Icon = "fa-paint-brush" ,CreatedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Category { Id = 3, Name = "Marketing", Description = "Các khóa học về marketing", Icon = "fa-bullhorn" ,CreatedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Category { Id = 4, Name = "Kinh doanh", Description = "Các khóa học về kinh doanh", Icon = "fa-briefcase",CreatedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)  }
        };
            builder.Entity<Category>().HasData(categories);
        }
    }
}
