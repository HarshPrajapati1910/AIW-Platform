using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIW.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentReviewReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DocumentReviewReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OverallScore = table.Column<int>(type: "int", nullable: false),
                    ExecutiveSummary = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SecurityScore = table.Column<int>(type: "int", nullable: false),
                    PerformanceScore = table.Column<int>(type: "int", nullable: false),
                    ArchitectureScore = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentReviewReports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DocumentReviewFinding",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Domain = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProblematicCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RecommendedPatch = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentReviewFinding", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentReviewFinding_DocumentReviewReports_ReportId",
                        column: x => x.ReportId,
                        principalTable: "DocumentReviewReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentReviewFinding_ReportId",
                table: "DocumentReviewFinding",
                column: "ReportId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentReviewFinding");

            migrationBuilder.DropTable(
                name: "DocumentReviewReports");
        }
    }
}
