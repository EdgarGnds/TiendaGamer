using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TiendaGamer.Migrations
{
    /// <inheritdoc />
    public partial class AddShippingToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ShippingAddress",
                table: "Orders",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ShippingCity",
                table: "Orders",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ShippingName",
                table: "Orders",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ShippingPostalCode",
                table: "Orders",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShippingAddress",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingCity",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingName",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingPostalCode",
                table: "Orders");
        }
    }
}
