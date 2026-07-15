using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OmniBlox.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Company_AddOtherIndustry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OtherIndustry",
                table: "Companies",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OtherIndustry",
                table: "Companies");
        }
    }
}
