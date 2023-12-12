
using System.Diagnostics;

namespace CancellationTokenBackGroudServiceSample
{
    public class TimedHostedService(ILogger<TimedHostedService> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Timed Hosted Service running.");

            using PeriodicTimer timer = new(TimeSpan.FromSeconds(10));

            try
            {
                var sw = new Stopwatch();
                sw.Start();
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    //validate user rule
                    if (AnyRule())
                    {
                        await DoWork(sw.Elapsed, logger, stoppingToken);
                    }
                    sw.Restart();
                }
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Timed Hosted Service is stopped.");
            }
        }

        private bool AnyRule()
        {
            return true;
        }

        private static async Task DoWork(TimeSpan elapsedtime, ILogger logger, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();   
            logger.LogInformation("Timed Hosted Service by DoWork is working. elapsedtime: {elapsedtime}", elapsedtime);
            await Task.CompletedTask;
        }
    }
}
