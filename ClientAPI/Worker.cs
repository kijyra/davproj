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
        string registryPath = @"SOFTWARE\Dallari\HardwareAgent";
        string valueName = "LastRunDate";

        _logger.LogInformation("Служба запущена. Интервал: {hours} ч.", _collectionIntervalHours);

        while (!stoppingToken.IsCancellationRequested)
        {
            DateTime lastRun = DateTime.MinValue;

            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registryPath))
            {
                if (key != null)
                {
                    string rawValue = key.GetValue(valueName)?.ToString();
                    DateTime.TryParse(rawValue, out lastRun);
                }
            }

            if ((DateTime.Now - lastRun).TotalHours >= _collectionIntervalHours)
            {
                try
                {
                    _logger.LogInformation("Условие по времени выполнено. Сбор данных...");

                    HardwareInfo info = _agent.CollectHardwareInfo();
                    info.CollectedAtUtc = DateTime.UtcNow;

                    await _apiClient.SendHardwareInfo(info);

                    using (RegistryKey key = Registry.LocalMachine.CreateSubKey(registryPath))
                    {
                        key.SetValue(valueName, DateTime.Now.ToString("O"));
                    }
                    _logger.LogInformation("Данные успешно зафиксированы в реестре.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка в цикле сбора/отправки");
                }
            }
            await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
        }
    }
}
