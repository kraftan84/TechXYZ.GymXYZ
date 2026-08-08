using MediatR;
using TechXyz.GymXyz.Application.Commands;

namespace TechXyz.GymXyz.WebApp.Services;

/// <summary>
/// Runs the three-month purge, once a day.
/// <para>
/// A hosted service rather than a scheduled job outside the app, because the
/// deployment target is SmarterASP shared hosting: there is no scheduler to hang
/// a task on and no second process to run it in. The trade-off is stated rather
/// than hidden — an app pool that is never woken never sweeps, so the first sweep
/// after a quiet week happens on the next visit rather than at midnight. For a
/// deletion promised "sous 3 mois", a sweep that can slip by a day is honest;
/// one that never runs would not be.
/// </para>
/// </summary>
public sealed class SpaceRequestPurgeService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(24);

    /// <summary>
    /// Long enough for the app to have finished starting. The database is
    /// recreated on startup in development, and sweeping while that runs would
    /// race it for no benefit.
    /// </summary>
    private static readonly TimeSpan StartupDelay = TimeSpan.FromMinutes(2);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SpaceRequestPurgeService> _logger;

    public SpaceRequestPurgeService(
        IServiceScopeFactory scopeFactory,
        ILogger<SpaceRequestPurgeService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await Task.Delay(StartupDelay, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                await SweepAsync(stoppingToken);
                await Task.Delay(Interval, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // The host is shutting down. Not a failure.
        }
    }

    private async Task SweepAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Its own scope: this outlives every request, and the scoped DbContext
            // it needs does not.
            using var scope = _scopeFactory.CreateScope();
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();

            var deleted = await sender.Send(new PurgeRefusedSpaceRequestsCommand(), cancellationToken);

            // Said out loud even at zero: "was the purge running" is otherwise
            // answered by reading the source of a deployed build.
            _logger.LogInformation(
                "Space request purge: {Deleted} refused request(s) older than three months deleted.",
                deleted);
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            // Swallowed on purpose: a failed sweep must not take the web app down
            // with it. Tomorrow's sweep picks up whatever this one missed.
            _logger.LogError(error, "The space request purge failed; it will be retried.");
        }
    }
}
