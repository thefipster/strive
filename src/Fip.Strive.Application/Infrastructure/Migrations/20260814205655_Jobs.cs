using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fip.Strive.Application.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Jobs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "jobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Kind = table.Column<string>(
                        type: "character varying(64)",
                        maxLength: 64,
                        nullable: false
                    ),
                    TargetKey = table.Column<string>(
                        type: "character varying(512)",
                        maxLength: 512,
                        nullable: false
                    ),
                    ComponentId = table.Column<string>(
                        type: "character varying(128)",
                        maxLength: 128,
                        nullable: false
                    ),
                    ComponentVersion = table.Column<int>(type: "integer", nullable: false),
                    State = table.Column<string>(
                        type: "character varying(16)",
                        maxLength: 16,
                        nullable: false
                    ),
                    Attempts = table.Column<int>(type: "integer", nullable: false),
                    Payload = table.Column<string>(type: "jsonb", nullable: true),
                    Error = table.Column<string>(type: "text", nullable: true),
                    ProgressCurrent = table.Column<int>(type: "integer", nullable: true),
                    ProgressTotal = table.Column<int>(type: "integer", nullable: true),
                    ProgressNote = table.Column<string>(
                        type: "character varying(1024)",
                        maxLength: 1024,
                        nullable: true
                    ),
                    EnqueuedUtc = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    StartedUtc = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    FinishedUtc = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_jobs", x => x.Id);
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_jobs_ComponentId_ComponentVersion",
                table: "jobs",
                columns: new[] { "ComponentId", "ComponentVersion" }
            );

            migrationBuilder.CreateIndex(
                name: "IX_jobs_Kind_TargetKey",
                table: "jobs",
                columns: new[] { "Kind", "TargetKey" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_jobs_State_EnqueuedUtc",
                table: "jobs",
                columns: new[] { "State", "EnqueuedUtc" }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "jobs");
        }
    }
}
