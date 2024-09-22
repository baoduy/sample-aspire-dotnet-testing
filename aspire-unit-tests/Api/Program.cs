using System.Reflection;
using Api.Configs;
using Api.Data;
using Api.Endpoints;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Db")));

builder.Services.AddMediatR(op => op.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => options.CustomSchemaIds(t =>
    t.GetCustomAttributes<SwaggerSchemaIdAttribute>().SingleOrDefault()?.SchemaId ??
    SwashbuckleHelpers.DefaultSchemaIdSelector(t)));
//Aspire Support
builder.AddServiceDefaults();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//Aspire Support
app.MapDefaultEndpoints();

app.UseHttpsRedirection();
app.MapProductEndpoints();
await app.RunAsync();

//This Startup endpoint for Unit Tests
namespace Api
{
    public class Program
    {
    }
}