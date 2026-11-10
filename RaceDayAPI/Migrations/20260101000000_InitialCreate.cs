using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814

namespace RaceDayAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserID    = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName  = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email     = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Password  = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Role      = table.Column<string>(type: "nvarchar(20)",  maxLength: 20,  nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserID);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    CategoryID   = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description  = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.CategoryID);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    EventID         = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventName       = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description     = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    EventDate       = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Location        = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Distance        = table.Column<decimal>(type: "decimal(6,2)", nullable: false),
                    MaxParticipants = table.Column<int>(type: "int", nullable: false),
                    EventType       = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "run"),
                    BannerImageUrl  = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CategoryID      = table.Column<int>(type: "int", nullable: false),
                    OrganiserID     = table.Column<int>(type: "int", nullable: false),
                    CreatedAt       = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.EventID);
                    table.ForeignKey(
                        name: "FK_Events_Categories_CategoryID",
                        column: x => x.CategoryID,
                        principalTable: "Categories",
                        principalColumn: "CategoryID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Events_Users_OrganiserID",
                        column: x => x.OrganiserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Enrolments",
                columns: table => new
                {
                    EnrolmentID   = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParticipantID = table.Column<int>(type: "int", nullable: false),
                    EventID       = table.Column<int>(type: "int", nullable: false),
                    EnrolledAt    = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status        = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enrolments", x => x.EnrolmentID);
                    table.ForeignKey(
                        name: "FK_Enrolments_Users_ParticipantID",
                        column: x => x.ParticipantID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Enrolments_Events_EventID",
                        column: x => x.EventID,
                        principalTable: "Events",
                        principalColumn: "EventID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Results",
                columns: table => new
                {
                    ResultID    = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EnrolmentID = table.Column<int>(type: "int", nullable: false),
                    FinishTime  = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Position    = table.Column<int>(type: "int", nullable: true),
                    Notes       = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Results", x => x.ResultID);
                    table.ForeignKey(
                        name: "FK_Results_Enrolments_EnrolmentID",
                        column: x => x.EnrolmentID,
                        principalTable: "Enrolments",
                        principalColumn: "EnrolmentID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Routes",
                columns: table => new
                {
                    RouteID       = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventID       = table.Column<int>(type: "int", nullable: false),
                    RouteMapURL   = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    StartPoint    = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EndPoint      = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ElevationGain = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    Description   = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Routes", x => x.RouteID);
                    table.ForeignKey(
                        name: "FK_Routes_Events_EventID",
                        column: x => x.EventID,
                        principalTable: "Events",
                        principalColumn: "EventID",
                        onDelete: ReferentialAction.Cascade);
                });

            // ── Indexes ──────────────────────────────────────────────────────
            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_CategoryName",
                table: "Categories",
                column: "CategoryName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_CategoryID",
                table: "Events",
                column: "CategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_Events_OrganiserID",
                table: "Events",
                column: "OrganiserID");

            migrationBuilder.CreateIndex(
                name: "IX_Enrolments_ParticipantID_EventID",
                table: "Enrolments",
                columns: new[] { "ParticipantID", "EventID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Enrolments_EventID",
                table: "Enrolments",
                column: "EventID");

            migrationBuilder.CreateIndex(
                name: "IX_Results_EnrolmentID",
                table: "Results",
                column: "EnrolmentID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Routes_EventID",
                table: "Routes",
                column: "EventID",
                unique: true);

            // ── Seed categories ───────────────────────────────────────────────
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "CategoryID", "CategoryName", "Description" },
                values: new object[,]
                {
                    { 1, "Road Running",  "Running events on tarred roads, ranging from 5km to full marathon" },
                    { 2, "Cycling",       "Road and trail cycling events for all fitness levels" },
                    { 3, "Walking",       "Casual and competitive walking events, great for all ages" },
                    { 4, "Trail Running", "Off-road running on natural terrain and mountain paths" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Routes");
            migrationBuilder.DropTable(name: "Results");
            migrationBuilder.DropTable(name: "Enrolments");
            migrationBuilder.DropTable(name: "Events");
            migrationBuilder.DropTable(name: "Categories");
            migrationBuilder.DropTable(name: "Users");
        }
    }
}
