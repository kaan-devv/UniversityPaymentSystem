using System.Diagnostics;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://localhost:5200");

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();

app.Use(async (context, next) =>
{
    Console.WriteLine($"\n--- [İSTEK BAŞLADI] ---");
    var watch = Stopwatch.StartNew();
    
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine($"GELEN: {context.Request.Method} {context.Request.Path}");
    Console.ResetColor();

    await next();

    watch.Stop();
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"BİTTİ: Status {context.Response.StatusCode} | {watch.ElapsedMilliseconds}ms");
    Console.ResetColor();
    Console.WriteLine($"--- [İSTEK BİTTİ] ---\n");
});

app.MapGet("/", () => "University Gateway Calisiyor! 🚀");

await app.UseOcelot();

app.Run();