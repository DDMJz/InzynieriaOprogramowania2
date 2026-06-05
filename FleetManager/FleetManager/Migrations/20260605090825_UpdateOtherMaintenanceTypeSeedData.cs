using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FleetManager.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOtherMaintenanceTypeSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "MaintenanceTypes",
                keyColumn: "Id",
                keyValue: 99,
                columns: new[] { "DefaultIntervalDays", "DefaultIntervalOdometer" },
                values: new object[] { null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "MaintenanceTypes",
                keyColumn: "Id",
                keyValue: 99,
                columns: new[] { "DefaultIntervalDays", "DefaultIntervalOdometer" },
                values: new object[] { 0, 0 });
        }
    }
}
