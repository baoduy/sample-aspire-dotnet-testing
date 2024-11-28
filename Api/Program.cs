using Api.Configs;
using Microsoft.EntityFrameworkCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

//TODO: Add feature management and allow to on/off Db migration on production.
builder.Services.AddHostedService<DbMigrationJob>();
// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("Db"));
    //NOTE: Workaround to ignore the error due to the bug of .NET9 here https://github.com/dotnet/efcore/issues/35110;
    options.ConfigureWarnings(x => x.Ignore(RelationalEventId.PendingModelChangesWarning));
});

//Swagger config
builder.Services.AddOpenApi();

//MediatR
builder.Services.AddMediatR(op => op.RegisterServicesFromAssembly(typeof(Program).Assembly));

//Aspire Support
builder.AddServiceDefaults();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

//Aspire Support
app.MapDefaultEndpoints();
app.MapProductEndpoints();
await app.RunAsync();

//This Startup endpoint for Unit Tests
namespace Api { public class Program; }