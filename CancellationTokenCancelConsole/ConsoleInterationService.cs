using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CancellationTokenCancelConsole
{
    internal class ConsoleInterationService(ILogger<ConsoleInterationService> logger, IHostApplicationLifetime applicationLifetime) : BackgroundService
    {
        private Task? TaskUI;
        private Task? TaskProcess;
        private bool AtivarProcesso;
        private readonly CancellationTokenSource cts = new();
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("ConsoleInteration Hosted Service running.");
            TaskUI = Task.Run(() =>
            {
                Console.WriteLine("Digite 1 para processar uma tarefa e 2 para Terminar");
                while (!applicationLifetime.ApplicationStopping.IsCancellationRequested)
                {
                    if (Console.KeyAvailable)
                    {
                        var kp = Console.ReadKey(true);
                        switch (kp.KeyChar)
                        {
                            case '1':
                                if (!AtivarProcesso)
                                {
                                    AtivarProcesso = true;
                                    while (AtivarProcesso)
                                    {
                                        stoppingToken.WaitHandle.WaitOne(1);
                                    }
                                    Console.WriteLine("Digite 1 para processar uma tarefa e 2 para Terminar");
                                }
                                break;
                            case '2':
                                cts.Cancel();
                                break;
                        }
                    }
                    //avoid cpu consumption(possible upscale)
                    stoppingToken.WaitHandle.WaitOne(1);
                }
            }, stoppingToken);

            TaskProcess = Task.Run(() =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    if (AtivarProcesso)
                    {
                        logger?.LogInformation($"TaskProcess is working {DateTime.Now}");
                        AtivarProcesso = false;
                    }
                    else
                    {
                        cts.Token.WaitHandle.WaitOne(1);
                    }
                }
                //after stop loop end Application (business rule)
                applicationLifetime.StopApplication();
            }, stoppingToken);
            await Task.CompletedTask;
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

            if (!TaskProcess?.IsCompleted ?? false)
            {
                //when Ctrl-C
                logger.LogWarning("TaskProcess not stopped!. send cancel");
                cts.Cancel();
                while (!TaskProcess?.IsCompleted ?? false)
                {
                    cancellationToken.WaitHandle.WaitOne(1);
                }
            }
            logger.LogInformation("TaskProcess stopped.");
            TaskProcess?.Dispose();

            cts.Dispose();
            logger.LogInformation("ConsoleInteration Hosted Service exit.");
            await base.StopAsync(cancellationToken);
        }
    }
}
