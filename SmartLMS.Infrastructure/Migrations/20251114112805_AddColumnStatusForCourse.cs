using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SmartLMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddColumnStatusForCourse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("321c6a4d-9730-474f-9347-1887a635e90e"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("c46143a1-0a49-44a0-bbfb-314a7badb851"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("c9960efc-7705-4281-b6dd-3e45557a6221"));

            migrationBuilder.AddColumn<bool>(
                name: "IsTimedQuiz",
                table: "Quizzes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Courses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("1d0fe904-5392-4f48-ad47-45226f1b319d"), null, "Instructor", "INSTRUCTOR" },
                    { new Guid("6d9c1299-7e7b-4689-848a-58a0623950f5"), null, "Student", "STUDENT" },
                    { new Guid("963e29cc-209b-4696-8bb1-f7631f74901e"), null, "Admin", "ADMIN" }
                });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 11, 14, 11, 28, 2, 39, DateTimeKind.Utc).AddTicks(6818));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2025, 11, 14, 11, 28, 2, 39, DateTimeKind.Utc).AddTicks(6825));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2025, 11, 14, 11, 28, 2, 39, DateTimeKind.Utc).AddTicks(6826));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedDate",
                value: new DateTime(2025, 11, 14, 11, 28, 2, 39, DateTimeKind.Utc).AddTicks(6828));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("1d0fe904-5392-4f48-ad47-45226f1b319d"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("6d9c1299-7e7b-4689-848a-58a0623950f5"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("963e29cc-209b-4696-8bb1-f7631f74901e"));

            migrationBuilder.DropColumn(
                name: "IsTimedQuiz",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Courses");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("321c6a4d-9730-474f-9347-1887a635e90e"), null, "Instructor", "INSTRUCTOR" },
                    { new Guid("c46143a1-0a49-44a0-bbfb-314a7badb851"), null, "Student", "STUDENT" },
                    { new Guid("c9960efc-7705-4281-b6dd-3e45557a6221"), null, "Admin", "ADMIN" }
                });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 9, 27, 4, 33, 54, 971, DateTimeKind.Utc).AddTicks(4560));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2025, 9, 27, 4, 33, 54, 971, DateTimeKind.Utc).AddTicks(4565));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2025, 9, 27, 4, 33, 54, 971, DateTimeKind.Utc).AddTicks(4567));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedDate",
                value: new DateTime(2025, 9, 27, 4, 33, 54, 971, DateTimeKind.Utc).AddTicks(4569));
        }
    }
}
