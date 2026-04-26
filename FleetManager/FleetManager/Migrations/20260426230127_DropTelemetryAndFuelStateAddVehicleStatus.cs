using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FleetManager.Migrations
{
    /// <inheritdoc />
    public partial class DropTelemetryAndFuelStateAddVehicleStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TelemetryLogs");

            migrationBuilder.DropColumn(
                name: "CurrentFuelLevel",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "LastGpsUpdate",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "LastKnownLatitude",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "LastKnownLongitude",
                table: "Vehicles");

            migrationBuilder.RenameColumn(
                name: "TotalCost",
                table: "FuelingEvents",
                newName: "Cost");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Vehicles",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Vehicles");

            migrationBuilder.RenameColumn(
                name: "Cost",
                table: "FuelingEvents",
                newName: "TotalCost");

            migrationBuilder.AddColumn<double>(
                name: "CurrentFuelLevel",
                table: "Vehicles",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastGpsUpdate",
                table: "Vehicles",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "LastKnownLatitude",
                table: "Vehicles",
                type: "double",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "LastKnownLongitude",
                table: "Vehicles",
                type: "double",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TelemetryLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    VehicleId = table.Column<int>(type: "int", nullable: false),
                    FuelLevel = table.Column<double>(type: "double", nullable: true),
                    Latitude = table.Column<double>(type: "double", nullable: false),
                    Longitude = table.Column<double>(type: "double", nullable: false),
                    RowVersion = table.Column<DateTime>(type: "timestamp(6)", rowVersion: true, nullable: false),
                    SpeedKph = table.Column<double>(type: "double", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelemetryLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TelemetryLogs_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_TelemetryLogs_VehicleId",
                table: "TelemetryLogs",
                column: "VehicleId");
        }
    }
}
