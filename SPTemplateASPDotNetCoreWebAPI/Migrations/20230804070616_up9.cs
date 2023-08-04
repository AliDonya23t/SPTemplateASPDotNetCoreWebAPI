using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SPTemplateASPDotNetCoreWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class up9 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedByIp",
                table: "RefreshToken",
                newName: "ReplacedByToken");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ReplacedByToken",
                table: "RefreshToken",
                newName: "CreatedByIp");
        }
    }
}
