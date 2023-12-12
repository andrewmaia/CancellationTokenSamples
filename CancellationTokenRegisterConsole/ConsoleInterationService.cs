using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CancellationTokenRegisterConsole
{
    internal class ConsoleInterationService(ILogger<ConsoleInterationService> logger, IHostApplicationLifetime applicationLifetime) : BackgroundService
    {
        private Task? TaskUI;
        private readonly CancellationTokenSource cts = new();

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            cts.Token.Register(RegraDeCancelamento);

            logger.LogInformation("ConsoleInteration Hosted Service running.");
            TaskUI = Task.Run(() =>
            {
                Console.WriteLine("Digite 1 para processar uma tarefa e 2 para Terminar");
                while (!stoppingToken.IsCancellationRequested && !applicationLifetime.ApplicationStopping.IsCancellationRequested)
                {
                    if (Console.KeyAvailable)
                    {
                        var kp = Console.ReadKey(true);
                        switch (kp.KeyChar)
                        {
                            case '1':
                                logger?.LogInformation($"TaskProcess is working {DateTime.Now}");
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
            await Task.CompletedTask;
        }

        private void RegraDeCancelamento()
        {
            //after stop loop end Application (business rule)
            logger?.LogInformation($"RegraDeCancelamento invoke by Register");
            applicationLifetime.StopApplication();
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

            cts.Dispose();
            logger.LogInformation("ConsoleInteration Hosted Service exit.");
            await base.StopAsync(cancellationToken);
        }
    }
}
