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
            migrationBuilder.CreateTable(
                name: "Inventory",
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
                        principalTable: "Zips",
                        principalColumn: "Hash",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ZipsHashed",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Hash = table.Column<string>(type: "text", nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    IndexedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ZipHash = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZipsHashed", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ZipsHashed_Zips_ZipHash",
                        column: x => x.ZipHash,
                        principalTable: "Zips",
                        principalColumn: "Hash");
                });

            migrationBuilder.CreateTable(
                name: "Data",
                columns: table => new
                {
                    Hash = table.Column<string>(type: "text", nullable: false),
                    Filepath = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_Data", x => x.Hash);
                    table.ForeignKey(
                        name: "FK_Data_Files_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Files",
                        principalColumn: "Hash",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FilesHashed",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Hash = table.Column<string>(type: "text", nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    IndexedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FileHash = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilesHashed", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FilesHashed_Files_FileHash",
                        column: x => x.FileHash,
                        principalTable: "Files",
                        principalColumn: "Hash");
                    table.ForeignKey(
                        name: "FK_FilesHashed_Files_Hash",
                        column: x => x.Hash,
                        principalTable: "Files",
                        principalColumn: "Hash",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FilesHashed_Zips_Hash",
                        column: x => x.Hash,
                        principalTable: "Zips",
                        principalColumn: "Hash",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Data_ParentId",
                table: "Data",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Data_ReferenceId",
                table: "Data",
                column: "ReferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_Files_ParentId",
                table: "Files",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Files_ReferenceId",
                table: "Files",
                column: "ReferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_FilesHashed_FileHash",
                table: "FilesHashed",
                column: "FileHash");

            migrationBuilder.CreateIndex(
                name: "IX_FilesHashed_Hash",
                table: "FilesHashed",
                column: "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_Inventory_Day",
                table: "Inventory",
                column: "Day");

            migrationBuilder.CreateIndex(
                name: "IX_Inventory_Month",
                table: "Inventory",
                column: "Month");

            migrationBuilder.CreateIndex(
                name: "IX_Inventory_Timestamp",
                table: "Inventory",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_Inventory_Year",
                table: "Inventory",
                column: "Year");

            migrationBuilder.CreateIndex(
                name: "IX_Zips_ReferenceId",
                table: "Zips",
                column: "ReferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_ZipsHashed_ZipHash",
                table: "ZipsHashed",
                column: "ZipHash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Data");

            migrationBuilder.DropTable(
                name: "FilesHashed");

            migrationBuilder.DropTable(
                name: "Inventory");

            migrationBuilder.DropTable(
                name: "ZipsHashed");

            migrationBuilder.DropTable(
                name: "Files");

            migrationBuilder.DropTable(
                name: "Zips");
        }
    }
}
