using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payment.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Depositing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "payment",
                table: "Wallets",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<Guid>(
                name: "AdministratorId",
                schema: "payment",
                table: "Transactions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_TouristId",
                schema: "payment",
                table: "Wallets",
                column: "TouristId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Wallets_TouristId",
                schema: "payment",
                table: "Wallets");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "payment",
                table: "Wallets");

            migrationBuilder.DropColumn(
                name: "AdministratorId",
                schema: "payment",
                table: "Transactions");
        }
    }
}
