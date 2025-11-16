using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fip.Strive.Harvester.Application.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "harvester-indexer");

            migrationBuilder.CreateTable(
                name: "Zips",
                schema: "harvester-indexer",
                columns: table => new
                {
                    Filename = table.Column<string>(type: "text", nullable: false),
                    Hash = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Zips", x => x.Filename);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Zips_Hash",
                schema: "harvester-indexer",
                table: "Zips",
                column: "Hash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Zips",
                schema: "harvester-indexer");
        }
    }
}
