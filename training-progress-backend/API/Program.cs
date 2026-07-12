using Core;
using Core.Persistence;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCoreServices(builder.Configuration);
builder.Services.AddFastEndpoints();
builder.Services.AddCors(options =>
{
    options.AddPolicy("LocalDev", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Clerk:Authority"];
        // Clerk JWTs do not include an audience claim by default. Configure one in
        // the Clerk dashboard and set Clerk:Audience in appsettings when required.
        options.TokenValidationParameters.ValidateAudience = false;
    });
builder.Services.AddAuthorization();
builder.Services.SwaggerDocument(o =>
{
    o.DocumentSettings = s =>
    {
        s.Title = "Training Progress API";
        s.Version = "v1";
    };
});

var app = builder.Build();

if (string.Equals(Environment.GetEnvironmentVariable("ENABLE_MIGRATIONS"), "true", StringComparison.OrdinalIgnoreCase))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

app.UseCors("LocalDev");
app.UseAuthentication();
app.UseAuthorization();
app.UseFastEndpoints();
app.UseSwaggerGen();

await app.RunAsync();

// Make Program visible to the Tests project for WebApplicationFactory
public partial class Program { }
