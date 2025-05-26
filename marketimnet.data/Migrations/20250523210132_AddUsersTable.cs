using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace marketimnet.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUsersTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 5, 24, 0, 1, 31, 862, DateTimeKind.Local).AddTicks(3319));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2025, 5, 24, 0, 1, 31, 862, DateTimeKind.Local).AddTicks(3330));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 5, 21, 14, 19, 57, 205, DateTimeKind.Local).AddTicks(564));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2025, 5, 21, 14, 19, 57, 205, DateTimeKind.Local).AddTicks(576));
        }
    }
}
