using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CancellationTokenCancelAfterConsole
{
    public class TimedHostedService(ILogger<TimedHostedService> logger, MyDoWork myDoWork) : BackgroundService
    {
        private static bool ProcessaTarefa;
        private static bool HasTimeout;
        private static readonly object lockvalue = new();
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Timed Hosted Service running.");

            using PeriodicTimer timer = new(TimeSpan.FromMilliseconds(1));

            try
            {
                var sw = new Stopwatch();
                sw.Start();
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    //validate user rule
                    if (AnyRule(stoppingToken))
                    {
                        var ok = await myDoWork.DoWork(HasTimeout, stoppingToken);
                        if (ok)
                        {
                            logger.LogInformation($"Timed Hosted Service by DoWork is working {DateTime.Now}");
                        }
                        else
                        {
                            logger.LogInformation($"Timed Hosted Service by DoWork Timeout!");
                        }
                        lock (lockvalue)
                        {
                            ProcessaTarefa = false;
                        }
                    }
                    if (HasTimeout)
                    {
                        lock (lockvalue)
                        {
                            HasTimeout = false;
                        }
                    }
                    sw.Restart();
                }
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Timed Hosted Service is stopped.");
            }
        }

        public static void AtivarProcesso(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            lock (lockvalue)
            {
                ProcessaTarefa = true;
            }
        }

        public static void AtivarTimeout(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            lock (lockvalue)
            {
                HasTimeout = true;
            }
        }

        public static bool AnyRule(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            lock (lockvalue)
            {
                return ProcessaTarefa;
            }
        }
    }
}
