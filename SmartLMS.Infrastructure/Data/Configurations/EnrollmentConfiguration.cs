using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SmartLMS.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLMS.Infrastructure.Data.Configurations
{
    public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
    {
        public void Configure(EntityTypeBuilder<Enrollment> builder)
        {
            builder.ToTable("Enrollments");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Progress)
                .HasColumnType("decimal(5,2)")
                .HasDefaultValue(0);

            builder.Property(e => e.FinalScore)
                .HasColumnType("decimal(5,2)");

            builder.Property(e => e.Status)
                .HasConversion<int>();

            builder.Property(e => e.EnrollmentDate)
                .HasDefaultValueSql("GETUTCDATE()");

            // Unique constraint: 1 student chỉ đăng ký 1 course 1 lần
            builder.HasIndex(e => new { e.StudentId, e.CourseId })
                .IsUnique()
                .HasDatabaseName("IX_Enrollments_Student_Course_Unique");

            // Indexes cho performance
            builder.HasIndex(e => new { e.StudentId, e.Status })
                .HasDatabaseName("IX_Enrollments_Student_Status");

            builder.HasIndex(e => e.EnrollmentDate)
                .HasDatabaseName("IX_Enrollments_Date");

            // Quan hệ với LessonProgress
            builder.HasMany(e => e.LessonProgress)
                .WithOne(lp => lp.Enrollment)
                .HasForeignKey(lp => lp.EnrollmentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
