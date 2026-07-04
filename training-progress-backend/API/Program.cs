using Core;
using FastEndpoints;
using FastEndpoints.Swagger;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCoreServices(builder.Configuration);
builder.Services.AddFastEndpoints();
builder.Services.SwaggerDocument(o =>
{
    o.DocumentSettings = s =>
    {
        s.Title = "Training Progress API";
        s.Version = "v1";
    };
});

var app = builder.Build();

app.UseFastEndpoints();
app.UseSwaggerGen();

app.Run();

// Make Program visible to the Tests project for WebApplicationFactory
public partial class Program { }
