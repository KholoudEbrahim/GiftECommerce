using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CartService.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGiftFunctionalityAndEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DeliveryAddressType",
                table: "Carts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "GiftDeliveryDate",
                table: "Carts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GiftMessage",
                table: "Carts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GiftWrapFee",
                table: "Carts",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "GiftWrapRequested",
                table: "Carts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsGift",
                table: "Carts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RecipientName",
                table: "Carts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecipientPhone",
                table: "Carts",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveryAddressType",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "GiftDeliveryDate",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "GiftMessage",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "GiftWrapFee",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "GiftWrapRequested",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "IsGift",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "RecipientName",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "RecipientPhone",
                table: "Carts");
        }
    }
}
