using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TMS.AuthService.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedSuperadminUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "Id", "DeletedAt", "Email", "IsDeleted", "PasswordHash", "Role", "UserName" },
                values: new object[] { 1, null, "superadmin@tms.ru", false, "$2b$12$b6doeIcInzPKmTANJeO7Euc3UE.VRkVLSY2E3gKLHbUvRLM4Y7NrS", 3, "superadmin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "User",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
