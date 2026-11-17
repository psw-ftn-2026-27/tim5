using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Games.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationChallenges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LocationChallenges",
                schema: "games",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Xp = table.Column<int>(type: "integer", nullable: false),
                    StartLocation_Latitude = table.Column<double>(type: "double precision", nullable: false),
                    StartLocation_Longitude = table.Column<double>(type: "double precision", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationChallenges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UploadedImages",
                schema: "games",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    StoredName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsLinked = table.Column<bool>(type: "boolean", nullable: false),
                    File_Content = table.Column<byte[]>(type: "bytea", nullable: false),
                    File_Size = table.Column<int>(type: "integer", nullable: false),
                    File_ContentType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UploadedImages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HiddenPoints",
                schema: "games",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ImageId = table.Column<Guid>(type: "uuid", nullable: false),
                    Location_Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Location_Longitude = table.Column<double>(type: "double precision", nullable: false),
                    LocationChallengeId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HiddenPoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HiddenPoints_LocationChallenges_LocationChallengeId",
                        column: x => x.LocationChallengeId,
                        principalSchema: "games",
                        principalTable: "LocationChallenges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HiddenPoints_LocationChallengeId",
                schema: "games",
                table: "HiddenPoints",
                column: "LocationChallengeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HiddenPoints",
                schema: "games");

            migrationBuilder.DropTable(
                name: "UploadedImages",
                schema: "games");

            migrationBuilder.DropTable(
                name: "LocationChallenges",
                schema: "games");
        }
    }
}
