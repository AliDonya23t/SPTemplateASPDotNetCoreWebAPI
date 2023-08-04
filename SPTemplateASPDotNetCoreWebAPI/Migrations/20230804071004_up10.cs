using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SPTemplateASPDotNetCoreWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class up10 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RevokedByIp",
                table: "RefreshToken");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RevokedByIp",
                table: "RefreshToken",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
