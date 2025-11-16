using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fip.Strive.Harvester.Application.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AddedFileInstance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Files",
                schema: "harvester-indexer",
                columns: table => new
                {
                    Filepath = table.Column<string>(type: "text", nullable: false),
                    ParentFilepath = table.Column<string>(type: "text", nullable: false),
                    Hash = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Files", x => x.Filepath);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Files_Hash",
                schema: "harvester-indexer",
                table: "Files",
                column: "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_Files_ParentFilepath",
                schema: "harvester-indexer",
                table: "Files",
                column: "ParentFilepath");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Files",
                schema: "harvester-indexer");
        }
    }
}
