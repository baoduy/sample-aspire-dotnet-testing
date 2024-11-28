using Aspire.Tests.Extensions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Aspire.Tests.Fixtures;

/**
 * ApiFixture is a test fixture class that sets up the necessary environment for integration tests.
 * It extends WebApplicationFactory<Api.Program> and implements IAsyncLifetime to manage the lifecycle
 * of the test environment.
 *
 * This class is responsible for:
 * - Setting up a PostgreSQL server resource.
 * - Configuring the host with the necessary connection strings.
 * - Ensuring the database is created before tests run.
 * - Starting and stopping the application host.
 * - Cleaning up resources after tests are completed.
 *
 * Usage:
 * - This class is used as a fixture in xUnit tests to provide a consistent and isolated test environment.
 */
public sealed class ApiFixture : WebApplicationFactory<Api.Program>, IAsyncLifetime
{
    private readonly DistributedApplication _app;
    private readonly IResourceBuilder<PostgresServerResource> _postgres;
    private string? _postgresConnectionString;

    /**
     * Constructor for ApiFixture.
     * Initializes the DistributedApplicationOptions and sets up the PostgreSQL server resource.
     */
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

    /**
     * Creates and configures the host for the application.
     * Adds the PostgreSQL connection string to the host configuration.
     * Ensures the database is created before returning the host.
     *
     * @param builder The IHostBuilder instance.
     * @return The configured IHost instance.
     */
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(config =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ConnectionStrings:Db", _postgresConnectionString },
            });
        });

        var host = base.CreateHost(builder);
        host.EnsureDbCreated().GetAwaiter().GetResult();
        return host;
    }

    /**
     * Disposes the resources used by the fixture asynchronously.
     * Stops the application host and disposes of it.
     */
    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _app.StopAsync();
        await _app.DisposeAsync();
    }

    /**
     * Initializes the fixture asynchronously.
     * Starts the application host and waits for the PostgreSQL resource to be in the running state.
     * Retrieve the PostgreSQL connection string.
     */
    public async Task InitializeAsync()
    {
        await _app.StartAsync();
        await _app.WaitForResourcesAsync();
        _postgresConnectionString = await _postgres.Resource.GetConnectionStringAsync();

        // Ensure that the PostgreSQL database is fully initialized before proceeding.
        // This is crucial, especially in CI/CD environments, to prevent tests from failing due to timing issues.
        await Task.Delay(TimeSpan.FromSeconds(5));
    }
}