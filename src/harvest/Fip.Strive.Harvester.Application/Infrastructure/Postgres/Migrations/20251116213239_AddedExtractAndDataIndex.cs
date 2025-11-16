using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fip.Strive.Harvester.Application.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AddedExtractAndDataIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Filename",
                schema: "harvester-indexer",
                table: "Zips",
                newName: "Filepath");

            migrationBuilder.CreateTable(
                name: "Data",
                schema: "harvester-indexer",
                columns: table => new
                {
                    Filepath = table.Column<string>(type: "text", nullable: false),
                    Hash = table.Column<string>(type: "text", nullable: false),
                    ParentHash = table.Column<string>(type: "text", nullable: false),
                    ParentFilepath = table.Column<string>(type: "text", nullable: false),
                    Source = table.Column<int>(type: "integer", nullable: false),
                    IsDay = table.Column<bool>(type: "boolean", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Data", x => x.Filepath);
                });

            migrationBuilder.CreateTable(
                name: "Extracts",
                schema: "harvester-indexer",
                columns: table => new
                {
                    Hash = table.Column<string>(type: "text", nullable: false),
                    Filepath = table.Column<string>(type: "text", nullable: false),
                    Source = table.Column<int>(type: "integer", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Extracts", x => x.Hash);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Data_Hash",
                schema: "harvester-indexer",
                table: "Data",
                column: "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_Data_IsDay",
                schema: "harvester-indexer",
                table: "Data",
                column: "IsDay");

            migrationBuilder.CreateIndex(
                name: "IX_Data_ParentFilepath",
                schema: "harvester-indexer",
                table: "Data",
                column: "ParentFilepath");

            migrationBuilder.CreateIndex(
                name: "IX_Data_Source",
                schema: "harvester-indexer",
                table: "Data",
                column: "Source");

            migrationBuilder.CreateIndex(
                name: "IX_Data_Timestamp",
                schema: "harvester-indexer",
                table: "Data",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_Extracts_Filepath",
                schema: "harvester-indexer",
                table: "Extracts",
                column: "Filepath");

            migrationBuilder.CreateIndex(
                name: "IX_Extracts_Source",
                schema: "harvester-indexer",
                table: "Extracts",
                column: "Source");

            migrationBuilder.CreateIndex(
                name: "IX_Extracts_Version",
                schema: "harvester-indexer",
                table: "Extracts",
                column: "Version");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Data",
                schema: "harvester-indexer");

            migrationBuilder.DropTable(
                name: "Extracts",
                schema: "harvester-indexer");

            migrationBuilder.RenameColumn(
                name: "Filepath",
                schema: "harvester-indexer",
                table: "Zips",
                newName: "Filename");
        }
    }
}
