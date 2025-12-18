using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CategoryService.Migrations
{
    /// <inheritdoc />
    public partial class AddBestSellerTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Rating",
                table: "Products",
                type: "decimal(3,2)",
                precision: 3,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "RatingCount",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalSales",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Products_TotalSales",
                table: "Products",
                column: "TotalSales");

            migrationBuilder.CreateIndex(
                name: "IX_Products_ViewCount",
                table: "Products",
                column: "ViewCount");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_TotalSales",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_ViewCount",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "RatingCount",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "TotalSales",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "Products");
        }
    }
}
