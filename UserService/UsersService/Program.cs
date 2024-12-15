using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Reflection;
using UsersService.Data;
using UsersService.Middlewares;

var builder = WebApplication.CreateBuilder(args);

string seqUrl = Environment.GetEnvironmentVariable("SEQ_URL");
if (String.IsNullOrEmpty(seqUrl))
    seqUrl = "http://seq:5341";

string pgConnStr = Environment.GetEnvironmentVariable("PG_CONN_STR");
if (String.IsNullOrEmpty(pgConnStr))
    pgConnStr = builder.Configuration.GetConnectionString("PgDb");

builder.Host.UseSerilog((context, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration)
    .Enrich.WithProperty("ApplicationName", Assembly.GetEntryAssembly().GetName().Name)
    .WriteTo.Seq(seqUrl);
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddTransient<LogContextMiddleware>();

builder.Services.AddDbContext<UsersDbContext>(options =>
    options.UseNpgsql(pgConnStr));

builder.Services.AddHealthChecks().AddNpgSql(pgConnStr);

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<LogContextMiddleware>();

app.UseSerilogRequestLogging();

app.MapHealthChecks("hc", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapGet("/health", () =>
{
    try
    {
        Log.Information($"Health status requested {Environment.MachineName}");

        Assembly assembly = Assembly.GetExecutingAssembly();
        System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);

        return Results.Ok(new
        {
            status = "OK",
            version = fvi.FileVersion,
            machinename = Environment.MachineName,
            osversion = Environment.OSVersion.VersionString,
            processid = Environment.ProcessId,
            timestamp = DateTime.Now,
            pgconnstr = pgConnStr,
            sequrl = seqUrl
        });
    }
    catch (Exception ex)
    {
        return Results.Ok(new
        {
            status = "BAD",
            machinename = Environment.MachineName,
            osversion = Environment.OSVersion.VersionString,
            processid = Environment.ProcessId,
            message = ex.Message
        });
    }
})
.WithName("GetHealth")
.WithOpenApi();

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
