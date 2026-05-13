using FleetManager.Services;
using FleetManager.Strategies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[assembly: ApiConventionType(typeof(FleetManager.Conventions.FleetApiConventions))]

namespace FleetManager
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection"); //znajduje w pliku appsettings.json w sekcji ConnectionStrigs

            builder.Services.AddDbContext<FleetManager.Data.AppDbContext>(
                (options) => options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
            );

            // dodawanie serwisow

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddOpenApi();

            builder.Services.AddScoped<FleetManager.Services.IFuelingService, FleetManager.Services.FuelingService>();
            builder.Services.AddScoped<FleetManager.Services.IMaintenanceService, FleetManager.Services.MaintenanceService>();
            builder.Services.AddScoped<IMaintenanceEvaluationService, MaintenanceEvaluationService>();
            builder.Services.AddScoped<IVehicleMaintenanceStatusService, VehicleMaintenanceStatusService>();
            // rejestracja strategi
            builder.Services.AddTransient<TimeOnlyStrategy>();
            builder.Services.AddTransient<DistanceOnlyStrategy>();
            builder.Services.AddTransient<CompositeStrategy>();
            // Rejestracja interfejsu nadrzędnego dla pętli Resolvera
            builder.Services.AddTransient<IMaintenanceStrategy, TimeOnlyStrategy>();
            builder.Services.AddTransient<IMaintenanceStrategy, DistanceOnlyStrategy>();
            builder.Services.AddTransient<IMaintenanceStrategy, CompositeStrategy>();

            //symulator telemetrii:
            builder.Services.AddHostedService<FleetManager.Workers.TelemetrySimulatorWorker>();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
