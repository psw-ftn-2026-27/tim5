using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Social.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBlogImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Images",
                schema: "social",
                table: "Blogs");

            migrationBuilder.AddColumn<List<Guid>>(
                name: "ImageIds",
                schema: "social",
                table: "Blogs",
                type: "uuid[]",
                nullable: false,
                defaultValueSql: "'{}'");

            migrationBuilder.CreateTable(
                name: "UploadedImages",
                schema: "social",
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UploadedImages",
                schema: "social");

            migrationBuilder.DropColumn(
                name: "ImageIds",
                schema: "social",
                table: "Blogs");

            migrationBuilder.AddColumn<List<string>>(
                name: "Images",
                schema: "social",
                table: "Blogs",
                type: "text[]",
                nullable: false);
        }
    }
}
