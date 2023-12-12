using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CancellationTokenCancelAfterConsole
{
    internal class ConsoleInterationService(ILogger<ConsoleInterationService> logger, IHostApplicationLifetime applicationLifetime) : BackgroundService
    {
        private Task? TaskUI;
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("ConsoleInteration Hosted Service running.");
            TaskUI = Task.Run(async () =>
            {
                Console.WriteLine("Digite 1 para processar uma tarefa e 2 para processar uma tarefa com timeout(após 10sec.) 3 para Terminar");
                while (!stoppingToken.IsCancellationRequested && !applicationLifetime.ApplicationStopping.IsCancellationRequested)
                {
                    if (Console.KeyAvailable)
                    {
                        var kp = Console.ReadKey(true);
                        long result = -1;
                        switch (kp.KeyChar)
                        {
                            case '1':
                                result = await ProcessaMensagem(false, stoppingToken);
                                break;
                            case '2':
                                {
                                    using var ctstimeout = new CancellationTokenSource();
                                    ctstimeout.CancelAfter(10000);
                                    result = await ProcessaMensagem(true, ctstimeout.Token);
                                }
                                break;
                            case '3':
                                //end Application (business rule)
                                applicationLifetime.StopApplication();
                                break;
                        }
                        if (result == 0)
                        {
                            logger?.LogInformation($"TaskProcess is working {DateTime.Now}");
                        }
                        else if (result > 0)
                        {
                            logger?.LogInformation($"TaskProcess is working but has timeout({TimeSpan.FromMilliseconds(result)})");
                        }
                        if (result >= 0)
                        {
                            stoppingToken.WaitHandle.WaitOne(1);
                            Console.WriteLine("Digite 1 para processar uma tarefa e 2 para processar uma tarefa com timeout(após 10sec.) 3 para Terminar");
                        }
                    }
                    //avoid cpu consumption(possible upscale)
                    stoppingToken.WaitHandle.WaitOne(1);
                }
            }, stoppingToken);
            await Task.CompletedTask;
        }

        private async Task<long> ProcessaMensagem(bool exectimeout, CancellationToken token)
        {
            logger?.LogInformation($"TaskProcess waiting start...");
            long Elapsed = 0;
            if (exectimeout)
            {
                var sw = Stopwatch.StartNew();
                try
                {
                    token.WaitHandle.WaitOne(11000);
                    token.ThrowIfCancellationRequested();
                }
                catch (OperationCanceledException)
                {
                    Elapsed = sw.ElapsedMilliseconds;
                }
            }
            return await Task.FromResult(Elapsed);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (!TaskUI?.IsCompleted ?? false)
            {
                //when Ctrl-C
                logger.LogWarning("TaskUI not stopped!. Waiting stop");
                while (!TaskUI?.IsCompleted ?? false)
                {
                    cancellationToken.WaitHandle.WaitOne(1);
                }
            }
            logger.LogInformation("TaskUI stopped.");
            TaskUI?.Dispose();
            logger.LogInformation("ConsoleInteration Hosted Service exit.");
            await base.StopAsync(cancellationToken);
        }
    }
}
