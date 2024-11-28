namespace Api.Configs;

/// <summary>
/// Represents a hosted service that runs database migrations on application startup.
/// </summary>
public class DbMigrationJob : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DbMigrationJob> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DbMigrationJob"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to create and manage services.</param>
    /// <param name="logger">The logger used to log information and errors.</param>
    public DbMigrationJob(IServiceProvider serviceProvider, ILogger<DbMigrationJob> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Starts the database migration process.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
    /// <returns>A task that represents the asynchronous start operation.</returns>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Create a scope to manage the lifecycle of the services.
        await using var scope = _serviceProvider.CreateAsyncScope();

        // Get the application database context from the service provider.
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        try
        {
            // Apply any pending migrations to the database.
            // Note: For demo purposes, EnsureCreatedAsync is used. In production, MigrateAsync should be used instead.
            await db.Database.MigrateAsync(cancellationToken: cancellationToken);

            _logger.LogInformation("Database migration has been run successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while running the database migration.");
        }
    }

    /// <summary>
    /// Stops the database migration process.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
    /// <returns>A task that represents the asynchronous stop operation.</returns>
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
