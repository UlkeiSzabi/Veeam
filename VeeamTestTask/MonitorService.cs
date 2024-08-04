using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

public class MonitorService : BackgroundService
{
    private readonly string _processName;
    private readonly TimeSpan _checkInterval;
    private readonly TimeSpan _lifeTime;
    private readonly CancellationToken _cancellationToken;
    private readonly ILogger<MonitorService> _logger;

    public MonitorService(string processName, TimeSpan checkInterval, TimeSpan lifeTime, CancellationToken cancellationToken, ILogger<MonitorService> logger)
    {
        _processName = processName;
        _checkInterval = checkInterval;
        _lifeTime = lifeTime;
        _cancellationToken = cancellationToken;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Link the provided stoppingToken with our own cancellation token
        using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, _cancellationToken))
        {
            while (!linkedCts.Token.IsCancellationRequested)
            {
                MonitorProcess();

                try
                {
                    // Wait for the specified interval before checking again
                    await Task.Delay(_checkInterval, linkedCts.Token);
                }
                catch (TaskCanceledException)
                {
                    _logger.LogInformation("Task was cancelled.");
                    break;
                }
            }
        }
    }

    void MonitorProcess()
    {
        var processes = Process.GetProcessesByName(_processName);
        if (processes.Length > 0)
        {
            foreach (var process in processes)
            {
                TimeSpan runtime = DateTime.Now - process.StartTime;
                _logger.LogInformation($"Process '{_processName}' with ID {process.Id} has been running for {runtime.TotalSeconds} seconds.");

                if(runtime >= _lifeTime)
                {
                    process.Kill();
                    _logger.LogInformation($"Process '{_processName}' with ID {process.Id} has been killed");
                }
            }
        }
        else
        {
            _logger.LogWarning($"No running processes found with the name '{_processName}'.");
        }
    }
}