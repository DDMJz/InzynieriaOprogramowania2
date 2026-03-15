using Usos.Components;
using Usos.Services;

namespace Usos;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        var app = builder.Build();

        try
        {
            using var connection = Conexion.CreateConexion();
            connection.Open();
            Console.WriteLine("Polaczono z baza MySQL");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Blad połączenia z baza: {ex.Message}");
        }

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseAntiforgery();

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.Run();
    }
}




/*
 * 1. Polaczenie z baza danych 
 * 2. Manipulacja danymi z poziomu C#
 * 3. Requesty, get,post,delete,update
 * 4. Manipulacja html, pobranie wartosci z pol, wysylanie itd.
 * 5. Generowanie html
 * 
 * 
 * 
 * 
 * 
 * 
 * */








