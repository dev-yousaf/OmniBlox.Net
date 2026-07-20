using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OmniBlox.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameStockColumn : Migration
    {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "StockQuantity",
            table: "Products",
            newName: "Stock");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "Stock",
            table: "Products",
            newName: "StockQuantity");
    }
    }
}
