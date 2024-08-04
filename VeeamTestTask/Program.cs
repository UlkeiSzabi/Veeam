
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

class Program
{
    public static async Task Main(String[] args)
    {
        // Parse command-line arguments for process name and check interval
        string processName = "notepad"; // Default process name
        int checkInterval = 1; // Default check interval in minutes
        int lifeTime = 5; // Default lifetime in minutes


        if (args.Length < 3 || !int.TryParse(args[1], out int lifeTime_param) || !int.TryParse(args[2], out int interval_param))
        {
            throw new ArgumentException("There must be exactly 4 arguments " +
                "| Ex: VeeamTestTask.exe Notepad 4 1\n" +
                " VeeaTestTask.exe <Program> <int>(lifeTime) <int>checkInterval");
        }
        else
        {
            checkInterval = interval_param;
            lifeTime = lifeTime_param;
            processName = args[0];
        }

        using var cancellationTokenSource = new CancellationTokenSource();
        using var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
                services.AddHostedService(provider =>
                    new MonitorService(
                        processName,
                        TimeSpan.FromMinutes(checkInterval),
                        TimeSpan.FromMinutes(lifeTime),
                        cancellationTokenSource.Token,
                        provider.GetRequiredService<ILogger<MonitorService>>())))
            .ConfigureLogging(logging =>
            {
                logging.AddConsole();
            })
            .Build();

        // Start the host
        var runTask = host.RunAsync();

        //For redirected output (Ex: Bash, Cygwin etc.)
        if (Console.IsOutputRedirected)
        {
            Console.WriteLine("Console is redirected. Press Enter to stop the service...");
            Console.ReadLine(); // Wait for Enter key instead of any key
        }
        else // For integrated terminal where console readkey is available
        {
            Console.WriteLine("Press Q key to stop the service...");
            do
            {
                while (!Console.KeyAvailable)
                {
                    // Wait for stopping signal
                }
            } while (Console.ReadKey(true).Key != ConsoleKey.Q);
        }

        // Signal cancellation
        cancellationTokenSource.Cancel();

        // Wait for the host to shut down
        await host.StopAsync();
        await runTask;
    }

}
