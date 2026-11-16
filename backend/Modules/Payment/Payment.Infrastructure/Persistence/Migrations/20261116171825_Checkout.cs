using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payment.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Checkout : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "AdministratorId",
                schema: "payment",
                table: "Transactions",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "EntityId",
                schema: "payment",
                table: "Transactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "payment",
                table: "Transactions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "payment",
                table: "ShoppingCarts",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.CreateTable(
                name: "TourPurchaseTokens",
                schema: "payment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TouristId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    PurchasedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TourPurchaseTokens", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TourPurchaseTokens_TouristId_EntityId",
                schema: "payment",
                table: "TourPurchaseTokens",
                columns: new[] { "TouristId", "EntityId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TourPurchaseTokens",
                schema: "payment");

            migrationBuilder.DropColumn(
                name: "EntityId",
                schema: "payment",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "Name",
                schema: "payment",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "payment",
                table: "ShoppingCarts");

            migrationBuilder.AlterColumn<Guid>(
                name: "AdministratorId",
                schema: "payment",
                table: "Transactions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
