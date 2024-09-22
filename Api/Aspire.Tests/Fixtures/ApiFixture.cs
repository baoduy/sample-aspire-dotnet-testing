using Aspire.Hosting;
using Aspire.Tests.Extensions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Aspire.Tests.Fixtures;

public sealed class ApiFixture : WebApplicationFactory<Api.Program>, IAsyncLifetime
{
    private readonly IHost _app;

    private readonly IResourceBuilder<PostgresServerResource> _postgres;
    private string? _postgresConnectionString;

    public ApiFixture()
    {
        var options = new DistributedApplicationOptions
        {
            AssemblyName = typeof(ApiFixture).Assembly.FullName,
            DisableDashboard = true
        };
        var builder = DistributedApplication.CreateBuilder(options);

        _postgres = builder.AddPostgres("postgres").PublishAsConnectionString();
        _app = builder.Build();
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(config =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ConnectionStrings:Db", _postgresConnectionString },
            }!);
        });

        var host = base.CreateHost(builder);
        host.EnsureDbCreated().GetAwaiter().GetResult();
        return host;
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _app.StopAsync();
        if (_app is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync().ConfigureAwait(false);
        }
        else
        {
            _app.Dispose();
        }
    }

    public async Task InitializeAsync()
    {
        var resourceNotificationService = _app.Services.GetRequiredService<ResourceNotificationService>();
        await _app.StartAsync();

        await resourceNotificationService.WaitForResourceAsync(_postgres.Resource.Name, KnownResourceStates.Running);
        _postgresConnectionString = await _postgres.Resource.GetConnectionStringAsync();
    }
}