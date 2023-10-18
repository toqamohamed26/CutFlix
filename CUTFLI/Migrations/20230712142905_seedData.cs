using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CUTFLI.Migrations
{
    /// <inheritdoc />
    public partial class seedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "CreatedBy", "CreatedDate", "LastModifiedBy", "LastModifiedDate", "Name" },
                values: new object[] { 1, null, new DateTime(2023, 7, 12, 17, 29, 5, 771, DateTimeKind.Local).AddTicks(8130), null, null, "Admin" });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Address", "CreatedBy", "CreatedDate", "Email", "FullName", "Image", "LastModifiedBy", "LastModifiedDate", "Password", "PhoneNumber", "RoleId" },
                values: new object[] { 1, "Irbid", null, new DateTime(2023, 7, 12, 17, 29, 5, 771, DateTimeKind.Local).AddTicks(8291), "ezawaqleh1@gmail.com", "Ezz Eldeen", null, null, null, "AQAAAAEAACcQAAAAEL4OkpScomcQ8y0KPpodA16wC7jGznr8AxV1UolMwkFvTbbQO6tX7WVQ5lKD48YFgg==", "0795015117", 1 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
