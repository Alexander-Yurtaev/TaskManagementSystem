using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TMS.AuthService.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixPendingChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2b$12$b6doeIcInzPKmTANJeO7Euc3UE.VRkVLSY2E3gKLHbUvRLM4Y7NrS");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "superadmin");
        }
    }
}
