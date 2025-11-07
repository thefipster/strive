using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Fip.Strive.Indexing.Application.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ingestion-index");

            migrationBuilder.CreateTable(
                name: "Inventory",
                schema: "ingestion-index",
                columns: table => new
                {
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    Day = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inventory", x => x.Date);
                });

            migrationBuilder.CreateTable(
                name: "Zips",
                schema: "ingestion-index",
                columns: table => new
                {
                    Hash = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SignalledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SignalId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Zips", x => x.Hash);
                });

            migrationBuilder.CreateTable(
                name: "Files",
                schema: "ingestion-index",
                columns: table => new
                {
                    Hash = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SignalledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SignalId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentId = table.Column<string>(type: "text", nullable: false),
                    Classified = table.Column<bool>(type: "boolean", nullable: false),
                    ClassificationResult = table.Column<int>(type: "integer", nullable: false),
                    ClassifierHash = table.Column<string>(type: "text", nullable: true),
                    Source = table.Column<int>(type: "integer", nullable: true),
                    ClassfierVersion = table.Column<int>(type: "integer", nullable: false),
                    LastClassificationAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExtractorVersion = table.Column<int>(type: "integer", nullable: false),
                    LastExtractionAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Extractions = table.Column<int>(type: "integer", nullable: false),
                    ExtractionMinDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExtractionMaxDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Files", x => x.Hash);
                    table.ForeignKey(
                        name: "FK_Files_Zips_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "ingestion-index",
                        principalTable: "Zips",
                        principalColumn: "Hash",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ZipsHashed",
                schema: "ingestion-index",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Hash = table.Column<string>(type: "text", nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    IndexedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZipsHashed", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ZipsHashed_Zips_Hash",
                        column: x => x.Hash,
                        principalSchema: "ingestion-index",
                        principalTable: "Zips",
                        principalColumn: "Hash",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Data",
                schema: "ingestion-index",
                columns: table => new
                {
                    Filepath = table.Column<string>(type: "text", nullable: false),
                    Hash = table.Column<string>(type: "text", nullable: false),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Source = table.Column<int>(type: "integer", nullable: false),
                    SourceFile = table.Column<string>(type: "text", nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentId = table.Column<string>(type: "text", nullable: false),
                    SignalledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SignalId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Data", x => x.Filepath);
                    table.ForeignKey(
                        name: "FK_Data_Files_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "ingestion-index",
                        principalTable: "Files",
                        principalColumn: "Hash",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FilesHashed",
                schema: "ingestion-index",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Hash = table.Column<string>(type: "text", nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    IndexedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilesHashed", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FilesHashed_Files_Hash",
                        column: x => x.Hash,
                        principalSchema: "ingestion-index",
                        principalTable: "Files",
                        principalColumn: "Hash",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Data_ParentId",
                schema: "ingestion-index",
                table: "Data",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Data_ReferenceId",
                schema: "ingestion-index",
                table: "Data",
                column: "ReferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_Files_ParentId",
                schema: "ingestion-index",
                table: "Files",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Files_ReferenceId",
                schema: "ingestion-index",
                table: "Files",
                column: "ReferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_FilesHashed_Hash",
                schema: "ingestion-index",
                table: "FilesHashed",
                column: "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_Inventory_Day",
                schema: "ingestion-index",
                table: "Inventory",
                column: "Day");

            migrationBuilder.CreateIndex(
                name: "IX_Inventory_Month",
                schema: "ingestion-index",
                table: "Inventory",
                column: "Month");

            migrationBuilder.CreateIndex(
                name: "IX_Inventory_Timestamp",
                schema: "ingestion-index",
                table: "Inventory",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_Inventory_Year",
                schema: "ingestion-index",
                table: "Inventory",
                column: "Year");

            migrationBuilder.CreateIndex(
                name: "IX_Zips_ReferenceId",
                schema: "ingestion-index",
                table: "Zips",
                column: "ReferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_ZipsHashed_Hash",
                schema: "ingestion-index",
                table: "ZipsHashed",
                column: "Hash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Data",
                schema: "ingestion-index");

            migrationBuilder.DropTable(
                name: "FilesHashed",
                schema: "ingestion-index");

            migrationBuilder.DropTable(
                name: "Inventory",
                schema: "ingestion-index");

            migrationBuilder.DropTable(
                name: "ZipsHashed",
                schema: "ingestion-index");

            migrationBuilder.DropTable(
                name: "Files",
                schema: "ingestion-index");

            migrationBuilder.DropTable(
                name: "Zips",
                schema: "ingestion-index");
        }
    }
}
