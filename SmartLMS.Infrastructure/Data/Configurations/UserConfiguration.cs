using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartLMS.Core.Entities;

namespace SmartLMS.Infrastructure.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            // Bảng tên
            builder.ToTable("Users");

            // Cấu hình các trường
            builder.Property(u => u.FirstName)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(u => u.LastName)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(u => u.Avatar)
                .HasMaxLength(500);

            builder.Property(u => u.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");

            // Indexes để tăng tốc truy vấn
            builder.HasIndex(u => u.Email)
                .IsUnique();

            builder.HasIndex(u => u.UserName)
                .IsUnique();

            builder.HasIndex(u => u.CreatedDate);

            // Quan hệ với các bảng khác
            builder.HasMany(u => u.CreatedCourses)
                .WithOne(c => c.Instructor)
                .HasForeignKey(c => c.InstructorId)
                .OnDelete(DeleteBehavior.Restrict); // Không xóa user nếu còn courses

            builder.HasMany(u => u.Enrollments)
                .WithOne(e => e.Student)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade); // Xóa user thì xóa luôn enrollments
        }
    }
}
