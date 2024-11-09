using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Prometheus;
using Serilog.Formatting.Compact;
using Serilog;
using System.Text;
using UserServiceAPI.Data;
using Elastic.Serilog.Sinks;

var builder = WebApplication.CreateBuilder(args);

//Jwt configuration starts here
var jwtIssuer = builder.Configuration.GetSection("Jwt:Issuer").Get<string>();
var jwtKey = builder.Configuration.GetSection("Jwt:Key").Get<string>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
 .AddJwtBearer(options =>
 {
     options.TokenValidationParameters = new TokenValidationParameters
     {
         ValidateIssuer = true,
         ValidateAudience = true,
         ValidateLifetime = true,
         ValidateIssuerSigningKey = true,
         ValidIssuer = jwtIssuer,
         ValidAudience = jwtIssuer,
         IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
     };
 });

//var logger = new LoggerConfiguration()
//                                .ReadFrom.Configuration(builder.Configuration)
//                                .Enrich.FromLogContext()
//                                .WriteTo.Console(new CompactJsonFormatter())
//                                .WriteTo.Elasticsearch(new Serilog.Sinks.Elasticsearch.ElasticsearchSinkOptions(new Uri("http://localhost:9200"))
//                                {
//                                    AutoRegisterTemplate = true,
//                                })
//                                .CreateLogger();
//builder.Host.UseSerilog(logger);

//builder.Configuration.AddKeyPerFile("/etc/conf", false, true);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.EnableAnnotations();
});

string pgConnStr = Environment.GetEnvironmentVariable("PG_CONNECTION_STRING");
if (String.IsNullOrEmpty(pgConnStr))
    pgConnStr = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<UsersDbContext>(options => options.UseNpgsql(pgConnStr));

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
//}

//app.UseSerilogRequestLogging();

//Starting the metrics exporter, will expose "/metrics"
app.UseMetricServer();

app.UseHttpMetrics(options =>
{
    options.AddCustomLabel("host", context => context.Request.Host.Host);
});

app.UseAuthorization();

app.MapControllers();

app.Run();
