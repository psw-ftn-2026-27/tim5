using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payment.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class GiftCards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GiftCardId",
                schema: "payment",
                table: "Transactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GiftCards",
                schema: "payment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SenderId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipientId = table.Column<Guid>(type: "uuid", nullable: false),
                    Message = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RedeemedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Amount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiftCards", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GiftCards_RecipientId",
                schema: "payment",
                table: "GiftCards",
                column: "RecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_GiftCards_SenderId",
                schema: "payment",
                table: "GiftCards",
                column: "SenderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GiftCards",
                schema: "payment");

            migrationBuilder.DropColumn(
                name: "GiftCardId",
                schema: "payment",
                table: "Transactions");
        }
    }
}
