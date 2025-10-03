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
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.ToTable("Categories");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(c => c.Description)
                .HasMaxLength(500);

            builder.Property(c => c.Icon)
                .HasMaxLength(100);

            builder.Property(c => c.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");

            // Index cho tìm kiếm
            builder.HasIndex(c => c.Name);
            builder.HasIndex(c => c.IsActive);

            // Quan hệ parent-child
            builder.HasOne(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Quan hệ với Course
            builder.HasMany(c => c.Courses)
                .WithOne(co => co.Category)
                .HasForeignKey(co => co.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
