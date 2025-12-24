using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CartService.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCofig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Carts_AnonymousId_Status",
                table: "Carts");

            migrationBuilder.DropIndex(
                name: "IX_Carts_UserId_Status",
                table: "Carts");

            migrationBuilder.AlterColumn<string>(
                name: "ProductName",
                table: "CartItems",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "CartItems",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.CreateIndex(
                name: "IX_Carts_AnonymousId_Status",
                table: "Carts",
                columns: new[] { "AnonymousId", "Status" },
                unique: true,
                filter: "[Status] = 'Active' AND AnonymousId IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Carts_UserId_Status",
                table: "Carts",
                columns: new[] { "UserId", "Status" },
                unique: true,
                filter: "[Status] = 'Active' AND UserId IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Carts_AnonymousId_Status",
                table: "Carts");

            migrationBuilder.DropIndex(
                name: "IX_Carts_UserId_Status",
                table: "Carts");

            migrationBuilder.AlterColumn<string>(
                name: "ProductName",
                table: "CartItems",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "CartItems",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Carts_AnonymousId_Status",
                table: "Carts",
                columns: new[] { "AnonymousId", "Status" },
                unique: true,
                filter: "[Status] = 'Active'");

            migrationBuilder.CreateIndex(
                name: "IX_Carts_UserId_Status",
                table: "Carts",
                columns: new[] { "UserId", "Status" },
                unique: true,
                filter: "[Status] = 'Active'");
        }
    }
}
