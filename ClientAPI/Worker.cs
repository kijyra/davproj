using ClientAPI;
using HardwareAgent;
using HardwareShared;
using Microsoft.Extensions.Options;
using Microsoft.Win32;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly ClientAPI.HardwareAgent _agent;
    private readonly ApiClient _apiClient;
    private readonly int _collectionIntervalHours;
    private const string RegistryPath = @"SOFTWARE\Dallari\HardwareAgent";
    private const string ValueName = "LastRunDate";

    public Worker(ILogger<Worker> logger, ClientAPI.HardwareAgent agent, ApiClient apiClient, IOptions<Settings> options)
    {
        _logger = logger;
        _agent = agent;
        _apiClient = apiClient;
        _collectionIntervalHours = options.Value.CollectionIntervalHours;

        if (_collectionIntervalHours <= 0)
        {
            _logger.LogCritical("CollectionIntervalHours некорректен в appsettings.json.");
            throw new ArgumentException("CollectionIntervalHours must be > 0");
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Служба сбора данных запущена. Интервал: {hours} ч.", _collectionIntervalHours);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndCollectData();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в фоновом цикле сбора данных");
            }
            await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
        }
    }

    private async Task CheckAndCollectData()
    {
        DateTime lastRun = DateTime.MinValue;

        using (RegistryKey? key = Registry.LocalMachine.OpenSubKey(RegistryPath))
        {
            if (key != null)
            {
                string? rawValue = key.GetValue(ValueName)?.ToString();
                DateTime.TryParse(rawValue, out lastRun);
            }
        }

        if ((DateTime.Now - lastRun).TotalHours >= _collectionIntervalHours)
        {
            _logger.LogInformation("Условие по времени выполнено. Сбор данных о железе...");

            HardwareInfo info = _agent.CollectHardwareInfo();
            info.CollectedAtUtc = DateTime.UtcNow;

            await _apiClient.SendHardwareInfo(info);

            using (RegistryKey key = Registry.LocalMachine.CreateSubKey(RegistryPath))
            {
                key.SetValue(ValueName, DateTime.Now.ToString("O"));
            }
            _logger.LogInformation("Данные о железе успешно отправлены и зафиксированы.");
        }
    }
}
