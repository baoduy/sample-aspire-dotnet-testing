namespace Api.Configs;

public class DbMigrationJob(IServiceProvider serviceProvider,ILogger<DbMigrationJob>logger):IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        await using var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        //TODO: This is for demo purposes, Add Db migration code and changes this to MigrateAsync instead.
        await db.Database.EnsureCreatedAsync(cancellationToken: cancellationToken);

        logger.LogInformation("Db migration had been run successfully.");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}