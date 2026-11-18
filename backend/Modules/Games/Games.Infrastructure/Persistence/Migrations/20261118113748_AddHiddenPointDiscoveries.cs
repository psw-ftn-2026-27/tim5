using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Games.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddHiddenPointDiscoveries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Completion_CompletedAt",
                schema: "games",
                table: "LocationChallengeActivations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Completion_Xp",
                schema: "games",
                table: "LocationChallengeActivations",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid[]>(
                name: "DiscoveredPointIds",
                schema: "games",
                table: "LocationChallengeActivations",
                type: "uuid[]",
                nullable: false,
                defaultValue: new Guid[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Completion_CompletedAt",
                schema: "games",
                table: "LocationChallengeActivations");

            migrationBuilder.DropColumn(
                name: "Completion_Xp",
                schema: "games",
                table: "LocationChallengeActivations");

            migrationBuilder.DropColumn(
                name: "DiscoveredPointIds",
                schema: "games",
                table: "LocationChallengeActivations");
        }
    }
}
