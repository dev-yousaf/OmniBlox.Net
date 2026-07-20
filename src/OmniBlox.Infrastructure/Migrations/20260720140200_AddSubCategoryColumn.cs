using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OmniBlox.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSubCategoryColumn : Migration
    {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "SubCategory",
            table: "Products",
            type: "character varying(100)",
            maxLength: 100,
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "SubCategory",
            table: "Products");
    }
    }
}
