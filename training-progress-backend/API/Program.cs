using Core;
using FastEndpoints;
using FastEndpoints.Swagger;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCoreServices(builder.Configuration);
builder.Services.AddFastEndpoints();
builder.Services.AddAuthentication(); // Authentication scheme configured per environment (e.g. Clerk JWT)
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

app.UseAuthentication();
app.UseAuthorization();
app.UseFastEndpoints();
app.UseSwaggerGen();

app.Run();

// Make Program visible to the Tests project for WebApplicationFactory
public partial class Program { }
