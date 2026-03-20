using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FleetManager.Migrations
{
    /// <inheritdoc />
    public partial class AddOdometerToMaintenanceEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OdometerAtService",
                table: "MaintenanceEvents",
                newName: "TotalCost");

            migrationBuilder.AlterColumn<int>(
                name: "OdometerReading",
                table: "Vehicles",
                type: "int",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double");

            migrationBuilder.AlterColumn<int>(
                name: "DefaultIntervalOdometer",
                table: "MaintenanceTypes",
                type: "int",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double");

            migrationBuilder.AddColumn<int>(
                name: "OdometerReading",
                table: "MaintenanceEvents",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OdometerReading",
                table: "MaintenanceEvents");

            migrationBuilder.RenameColumn(
                name: "TotalCost",
                table: "MaintenanceEvents",
                newName: "OdometerAtService");

            migrationBuilder.AlterColumn<double>(
                name: "OdometerReading",
                table: "Vehicles",
                type: "double",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<double>(
                name: "DefaultIntervalOdometer",
                table: "MaintenanceTypes",
                type: "double",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
