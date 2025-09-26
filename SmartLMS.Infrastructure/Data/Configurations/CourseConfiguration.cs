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
    public class CourseConfiguration : IEntityTypeConfiguration<Course>
    {
        public void Configure(EntityTypeBuilder<Course> builder)
        {
            builder.ToTable("Courses");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Title)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(c => c.ShortDescription)
                .HasMaxLength(500);

            builder.Property(c => c.Description)
                .HasColumnType("NVARCHAR(MAX)");

            builder.Property(c => c.Thumbnail)
                .HasMaxLength(500);

            builder.Property(c => c.Price)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0);

            builder.Property(c => c.Level)
                .HasConversion<int>(); // Convert enum to int

            builder.Property(c => c.Requirements)
                .HasColumnType("NVARCHAR(MAX)");

            builder.Property(c => c.WhatYouWillLearn)
                .HasColumnType("NVARCHAR(MAX)");

            builder.Property(c => c.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");

            // Indexes quan trọng cho performance
            builder.HasIndex(c => new { c.CategoryId, c.IsPublished })
                .HasDatabaseName("IX_Courses_Category_Published");

            builder.HasIndex(c => c.InstructorId)
                .HasDatabaseName("IX_Courses_Instructor");

            builder.HasIndex(c => new { c.IsPublished, c.CreatedDate })
                .HasDatabaseName("IX_Courses_Published_Created");

            // Quan hệ với Lesson
            builder.HasMany(c => c.Lessons)
                .WithOne(l => l.Course)
                .HasForeignKey(l => l.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Quan hệ với Quiz
            builder.HasMany(c => c.Quizzes)
                .WithOne(q => q.Course)
                .HasForeignKey(q => q.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
