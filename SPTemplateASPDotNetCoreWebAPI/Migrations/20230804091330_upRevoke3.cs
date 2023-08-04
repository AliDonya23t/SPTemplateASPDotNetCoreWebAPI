using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SPTemplateASPDotNetCoreWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class upRevoke3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedByIp",
                table: "RefreshToken");

            migrationBuilder.DropColumn(
                name: "RevokedByIp",
                table: "RefreshToken");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedByIp",
                table: "RefreshToken",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RevokedByIp",
                table: "RefreshToken",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
