using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FleetManager.Migrations
{
    /// <inheritdoc />
    public partial class UpdateVehicleMetricsAndSeedMaintenanceTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "FuelConsumption",
                table: "Vehicles",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AlterColumn<int>(
                name: "DefaultIntervalOdometer",
                table: "MaintenanceTypes",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "DefaultIntervalDays",
                table: "MaintenanceTypes",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "SystemCode",
                table: "MaintenanceTypes",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "MaintenanceTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "SystemCode",
                value: "OIL_CHANGE");

            migrationBuilder.UpdateData(
                table: "MaintenanceTypes",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "DefaultIntervalOdometer", "SystemCode" },
                values: new object[] { null, "LEGAL_INSPECTION" });

            migrationBuilder.UpdateData(
                table: "MaintenanceTypes",
                keyColumn: "Id",
                keyValue: 99,
                columns: new[] { "Name", "SystemCode" },
                values: new object[] { "Inne / Naprawa dorazna", "OTHER" });

            migrationBuilder.InsertData(
                table: "MaintenanceTypes",
                columns: new[] { "Id", "DefaultIntervalDays", "DefaultIntervalOdometer", "Name", "SystemCode" },
                values: new object[] { 3, null, 30000, "Wymiana Klocków Hamulcowych", "BRAKE_PADS" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MaintenanceTypes",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DropColumn(
                name: "FuelConsumption",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "SystemCode",
                table: "MaintenanceTypes");

            migrationBuilder.AlterColumn<int>(
                name: "DefaultIntervalOdometer",
                table: "MaintenanceTypes",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DefaultIntervalDays",
                table: "MaintenanceTypes",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "MaintenanceTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "DefaultIntervalOdometer",
                value: 0);

            migrationBuilder.UpdateData(
                table: "MaintenanceTypes",
                keyColumn: "Id",
                keyValue: 99,
                column: "Name",
                value: "Inne");
        }
    }
}
