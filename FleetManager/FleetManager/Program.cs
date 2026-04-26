using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

[assembly: ApiConventionType(typeof(FleetManager.Conventions.FleetApiConventions))]

namespace FleetManager
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection"); //znajduje w pliku JSON w sekcji ConnectionStrig

            builder.Services.AddDbContext<FleetManager.Data.AppDbContext>(
                (options) => options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
            );

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            // business logic services
            builder.Services.AddScoped<FleetManager.Services.IFuelingService, FleetManager.Services.FuelingService>();
            builder.Services.AddScoped<FleetManager.Services.IMaintenanceService, FleetManager.Services.MaintenanceService>();
            //w tej chwili nie uzywane (logika nie jest na to gotowa):

            var app = builder.Build();

            // Configure the HTTP request pipeline.
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
