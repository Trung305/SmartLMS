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
    public class LessonConfiguration : IEntityTypeConfiguration<Lesson>
    {
        public void Configure(EntityTypeBuilder<Lesson> builder)
        {
            builder.ToTable("Lessons");

            builder.HasKey(l => l.Id);

            builder.Property(l => l.Title)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(l => l.Content)
                .HasColumnType("NVARCHAR(MAX)");

            builder.Property(l => l.VideoUrl)
                .HasMaxLength(500);

            builder.Property(l => l.Duration)
                .HasDefaultValue(0);

            builder.Property(l => l.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");

            // Indexes quan trọng cho performance
            builder.HasIndex(l => new { l.CourseId, l.OrderIndex })
                .HasDatabaseName("IX_Lessons_Course_Order");

            builder.HasIndex(l => new { l.CourseId, l.IsPublished })
                .HasDatabaseName("IX_Lessons_Course_Published");

            builder.HasIndex(l => l.IsPreview)
                .HasDatabaseName("IX_Lessons_Preview");

            // Quan hệ với Course
            builder.HasOne(l => l.Course)
                .WithMany(c => c.Lessons)
                .HasForeignKey(l => l.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
