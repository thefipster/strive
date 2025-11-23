using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fip.Strive.Harvester.Application.Infrastructure.Indexing.Migrations
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
                    Filepath = table.Column<string>(type: "text", nullable: false),
                    Hash = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Zips", x => x.Filepath);
                });

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
                    table.UniqueConstraint("AK_Files_Hash", x => x.Hash);
                    table.ForeignKey(
                        name: "FK_Files_Zips_ParentFilepath",
                        column: x => x.ParentFilepath,
                        principalSchema: "harvester-indexer",
                        principalTable: "Zips",
                        principalColumn: "Filepath",
                        onDelete: ReferentialAction.Cascade);
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
                    table.ForeignKey(
                        name: "FK_Extracts_Files_Hash",
                        column: x => x.Hash,
                        principalSchema: "harvester-indexer",
                        principalTable: "Files",
                        principalColumn: "Hash",
                        onDelete: ReferentialAction.Cascade);
                });

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
                    table.ForeignKey(
                        name: "FK_Sources_Files_Hash",
                        column: x => x.Hash,
                        principalSchema: "harvester-indexer",
                        principalTable: "Files",
                        principalColumn: "Hash",
                        onDelete: ReferentialAction.Cascade);
                });

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
                    table.ForeignKey(
                        name: "FK_Data_Extracts_ParentHash",
                        column: x => x.ParentHash,
                        principalSchema: "harvester-indexer",
                        principalTable: "Extracts",
                        principalColumn: "Hash",
                        onDelete: ReferentialAction.Cascade);
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
                name: "IX_Data_ParentHash",
                schema: "harvester-indexer",
                table: "Data",
                column: "ParentHash");

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
                name: "Data",
                schema: "harvester-indexer");

            migrationBuilder.DropTable(
                name: "Sources",
                schema: "harvester-indexer");

            migrationBuilder.DropTable(
                name: "Extracts",
                schema: "harvester-indexer");

            migrationBuilder.DropTable(
                name: "Files",
                schema: "harvester-indexer");

            migrationBuilder.DropTable(
                name: "Zips",
                schema: "harvester-indexer");
        }
    }
}
