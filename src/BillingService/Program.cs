using BillingService;
using BillingService.Data;
using BillingService.Endpoints;
using BillingService.Middlewares;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var jwtIssuer = builder.Configuration.GetSection("Jwt:Issuer").Get<string>();
var jwtKey = builder.Configuration.GetSection("Jwt:Key").Get<string>();

string seqUrl = Environment.GetEnvironmentVariable("SEQ_URL");
if (String.IsNullOrEmpty(seqUrl))
    seqUrl = "http://seq:5341";

string pgConnStr = Environment.GetEnvironmentVariable("PG_CONN_STR");
if (String.IsNullOrEmpty(pgConnStr))
    pgConnStr = builder.Configuration.GetConnectionString("PgDb");

//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
// .AddJwtBearer(options =>
// {
//     options.TokenValidationParameters = new TokenValidationParameters
//     {
//         ValidateIssuer = true,
//         ValidateAudience = true,
//         ValidateLifetime = true,
//         ValidateIssuerSigningKey = true,
//         ValidIssuer = jwtIssuer,
//         ValidAudience = jwtIssuer,
//         IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
//     };
// });

//builder.Services.AddAuthorization(options =>
//{
//    options.FallbackPolicy = new AuthorizationPolicyBuilder()
//      .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
//      .RequireAuthenticatedUser()
//      .Build();
//});

builder.Host.UseSerilog((context, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration)
    .Enrich.WithProperty("ApplicationName", Assembly.GetEntryAssembly().GetName().Name)
    .WriteTo.Seq(seqUrl);
});

builder.Services.AddApplication();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options => {
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 1safsfsdfdfd\"",
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddTransient<LogContextMiddleware>();

builder.Services.AddDbContext<BillingDbContext>(options =>
    options.UseNpgsql(pgConnStr));

builder.Services.AddScoped<IBillingRepository, BillingRepository>();

builder.Services.AddHealthChecks().AddNpgSql(pgConnStr);

var app = builder.Build();

//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
//}

app.UseMiddleware<LogContextMiddleware>();

app.UseSerilogRequestLogging();

//app.UseAuthentication();
//app.UseAuthorization();

app.MapHealthChecks("hc", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.AddBillingEndpoints(app.Services.GetRequiredService<IConfiguration>());

app.MapGet("/health", [AllowAnonymous] () =>
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
