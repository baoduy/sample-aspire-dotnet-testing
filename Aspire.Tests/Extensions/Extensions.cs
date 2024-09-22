using Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace Aspire.Tests.Extensions;

internal static class Extensions
{
    public static async Task EnsureDbCreated(this IHost app)
    {
        using var serviceScope = app.Services.CreateScope();
        var db = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();
    }
}