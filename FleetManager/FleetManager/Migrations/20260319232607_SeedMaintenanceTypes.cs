using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FleetManager.Migrations
{
    /// <inheritdoc />
    public partial class SeedMaintenanceTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "MaintenanceTypes",
                columns: new[] { "Id", "DefaultIntervalDays", "DefaultIntervalOdometer", "Name" },
                values: new object[,]
                {
                    { 1, 365, 15000, "Wymiana Oleju" },
                    { 2, 365, 0, "Przegląd Rejestracyjny" },
                    { 99, 0, 0, "Inne" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MaintenanceTypes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "MaintenanceTypes",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "MaintenanceTypes",
                keyColumn: "Id",
                keyValue: 99);
        }
    }
}
