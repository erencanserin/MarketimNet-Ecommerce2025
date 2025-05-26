using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace marketimnet.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateUsersTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AppUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedDate", "UserGuid" },
                values: new object[] { new DateTime(2025, 5, 24, 21, 1, 29, 361, DateTimeKind.Local).AddTicks(3656), new Guid("f07115de-743b-4cc8-9622-65e0168a3edc") });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 5, 24, 21, 1, 29, 361, DateTimeKind.Local).AddTicks(5338));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2025, 5, 24, 21, 1, 29, 361, DateTimeKind.Local).AddTicks(5344));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AppUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedDate", "UserGuid" },
                values: new object[] { new DateTime(2025, 5, 24, 20, 3, 43, 135, DateTimeKind.Local).AddTicks(6161), new Guid("6b54ce08-5734-482a-b19e-349635410d7c") });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 5, 24, 20, 3, 43, 135, DateTimeKind.Local).AddTicks(7839));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2025, 5, 24, 20, 3, 43, 135, DateTimeKind.Local).AddTicks(7846));
        }
    }
}
