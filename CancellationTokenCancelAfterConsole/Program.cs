using CancellationTokenCancelAfterConsole;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

internal class Program
{
    static void Main(string[] args)
    {
        var host = new HostBuilder()
                .ConfigureHostConfiguration(configHost =>
                {
                })
                .ConfigureLogging(configlog =>
                {
                    configlog.SetMinimumLevel(LogLevel.Information)
                        .AddFilter("Default", LogLevel.None)
                        .AddFilter("Microsoft", LogLevel.None)
                        .AddFilter("CancellationTokenCancelConsole", LogLevel.Information)
                        .AddConsole();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<ConsoleInterationService>();

                })
                .UseConsoleLifetime()
                .Build();

        //run the host
        host.Run();
    }
}


