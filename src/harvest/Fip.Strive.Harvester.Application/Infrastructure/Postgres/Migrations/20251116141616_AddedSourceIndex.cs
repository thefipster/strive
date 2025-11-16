using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fip.Strive.Harvester.Application.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AddedSourceIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Sources",
                schema: "harvester-indexer",
                columns: table => new
                {
                    Hash = table.Column<string>(type: "text", nullable: false),
                    Filepath = table.Column<string>(type: "text", nullable: false),
                    ClassifyHash = table.Column<string>(type: "text", nullable: false),
                    Source = table.Column<int>(type: "integer", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sources", x => x.Hash);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sources_ClassifyHash",
                schema: "harvester-indexer",
                table: "Sources",
                column: "ClassifyHash");

            migrationBuilder.CreateIndex(
                name: "IX_Sources_Filepath",
                schema: "harvester-indexer",
                table: "Sources",
                column: "Filepath");

            migrationBuilder.CreateIndex(
                name: "IX_Sources_Source",
                schema: "harvester-indexer",
                table: "Sources",
                column: "Source");

            migrationBuilder.CreateIndex(
                name: "IX_Sources_Version",
                schema: "harvester-indexer",
                table: "Sources",
                column: "Version");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Sources",
                schema: "harvester-indexer");
        }
    }
}
