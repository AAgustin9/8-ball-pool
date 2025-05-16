using Microsoft.EntityFrameworkCore;
using _8_ball_pool.Data;
using DotNetEnv;
using Amazon.S3;
using Amazon.Runtime;
using Amazon.Extensions.NETCore.Setup;
using _8_ball_pool.Services;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Cargar variables de entorno desde el archivo .env
Env.Load();

// PostgreSQL connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       "Host=localhost;Port=5432;Database=pooldb;Username=postgres;Password=postgres";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// AWS S3
builder.Services.AddDefaultAWSOptions(new AWSOptions
{
    Credentials = new BasicAWSCredentials(
        Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID"),
        Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY")),
    Region = Amazon.RegionEndpoint.GetBySystemName(
        Environment.GetEnvironmentVariable("AWS_REGION") ?? "us-east-1")
});
builder.Services.AddAWSService<IAmazonS3>();

// Add health checks
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString, name: "database")
    .AddCheck("self", () => HealthCheckResult.Healthy());

builder.Services.AddScoped<IS3Service, S3Service>();
// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    c.RoutePrefix = string.Empty;
});
}

// Add health endpoint
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = System.Text.Json.JsonSerializer.Serialize(
            new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    exception = e.Value.Exception?.Message,
                    duration = e.Value.Duration.ToString()
                })
            });
        await context.Response.WriteAsync(result);
    }
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();