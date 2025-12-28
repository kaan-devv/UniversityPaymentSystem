using System.Diagnostics;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();

app.Use(async (context, next) =>
{
    var requestTime = DateTime.UtcNow;
    var watch = Stopwatch.StartNew();

    var remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    var headers = string.Join("; ", context.Request.Headers.Select(h => $"{h.Key}={h.Value}"));
    var requestSize = context.Request.ContentLength ?? 0;

    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine($"\n--- [NEW REQUEST] {requestTime} ---");
    Console.WriteLine($"IP: {remoteIp} | Size: {requestSize} bytes");
    Console.WriteLine($"Request: {context.Request.Method} {context.Request.Path}{context.Request.QueryString}");
    Console.WriteLine($"Headers: {headers}");
    Console.ResetColor();

    await next();

    watch.Stop();
    var statusCode = context.Response.StatusCode;
    var authStatus = (statusCode == 401 || statusCode == 403) ? "FAILED" : "SUCCESS";

    Console.ForegroundColor = statusCode >= 400 ? ConsoleColor.Red : ConsoleColor.Green;
    Console.WriteLine($"[RESPONSE] Status: {statusCode} | Auth: {authStatus} | Duration: {watch.ElapsedMilliseconds}ms");
    Console.ResetColor();
    Console.WriteLine("----------------------------------\n");
});

app.MapGet("/", () => "University Gateway is Running! 🚀");

await app.UseOcelot();

app.Run();