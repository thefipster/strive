using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Fip.Strive.Application.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "catalog_entries",
                columns: table => new
                {
                    Hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    FirstSeenUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_catalog_entries", x => x.Hash);
                });

            migrationBuilder.CreateTable(
                name: "import_packages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ArchiveHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    FileName = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    UploadedUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    FileCount = table.Column<int>(type: "integer", nullable: false),
                    NewEntryCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_import_packages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "package_files",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PackageId = table.Column<Guid>(type: "uuid", nullable: false),
                    PathInArchive = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_package_files", x => x.Id);
                    table.ForeignKey(
                        name: "FK_package_files_catalog_entries_Hash",
                        column: x => x.Hash,
                        principalTable: "catalog_entries",
                        principalColumn: "Hash",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_package_files_import_packages_PackageId",
                        column: x => x.PackageId,
                        principalTable: "import_packages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_catalog_entries_FirstSeenUtc",
                table: "catalog_entries",
                column: "FirstSeenUtc");

            migrationBuilder.CreateIndex(
                name: "IX_import_packages_ArchiveHash",
                table: "import_packages",
                column: "ArchiveHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_import_packages_UploadedUtc",
                table: "import_packages",
                column: "UploadedUtc");

            migrationBuilder.CreateIndex(
                name: "IX_package_files_Hash",
                table: "package_files",
                column: "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_package_files_PackageId_PathInArchive",
                table: "package_files",
                columns: new[] { "PackageId", "PathInArchive" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "package_files");

            migrationBuilder.DropTable(
                name: "catalog_entries");

            migrationBuilder.DropTable(
                name: "import_packages");
        }
    }
}
