using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FIAP.AgroSolutions.Alerts.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialDatabse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlertRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RuleKey = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Severity = table.Column<int>(type: "int", nullable: false),
                    Kind = table.Column<int>(type: "int", nullable: false),
                    Metric = table.Column<int>(type: "int", nullable: false),
                    Operator = table.Column<int>(type: "int", nullable: false),
                    ThresholdValue = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    DurationMinutes = table.Column<int>(type: "int", nullable: true),
                    CooldownMinutes = table.Column<int>(type: "int", nullable: true),
                    MessageTemplate = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SecondaryMetric = table.Column<int>(type: "int", nullable: true),
                    SecondaryMinValue = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    SecondaryMaxValue = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Alerts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FieldId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Severity = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TriggeredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResolvedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alerts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FieldStates",
                columns: table => new
                {
                    FieldId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastReadingAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastSoilMoisturePercent = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    LastTemperatureC = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    LastRainMm = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FieldStates", x => x.FieldId);
                });

            migrationBuilder.CreateTable(
                name: "SensorReadings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FieldId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SoilMoisturePercent = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    TemperatureC = table.Column<decimal>(type: "decimal(6,2)", nullable: false),
                    RainMm = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    MeasuredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReceivedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SensorReadings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RuleStates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FieldId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RuleKey = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    WindowStartUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastTriggeredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AlertActive = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RuleStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RuleStates_FieldStates_FieldId",
                        column: x => x.FieldId,
                        principalTable: "FieldStates",
                        principalColumn: "FieldId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlertRules_RuleKey",
                table: "AlertRules",
                column: "RuleKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_FieldId_Type_TriggeredAtUtc",
                table: "Alerts",
                columns: new[] { "FieldId", "Type", "TriggeredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_RuleStates_FieldId_RuleKey",
                table: "RuleStates",
                columns: new[] { "FieldId", "RuleKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SensorReadings_FieldId_MeasuredAtUtc",
                table: "SensorReadings",
                columns: new[] { "FieldId", "MeasuredAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlertRules");

            migrationBuilder.DropTable(
                name: "Alerts");

            migrationBuilder.DropTable(
                name: "RuleStates");

            migrationBuilder.DropTable(
                name: "SensorReadings");

            migrationBuilder.DropTable(
                name: "FieldStates");
        }
    }
}
