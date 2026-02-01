using HardwareAgent;
using HardwareShared;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace ClientAPI
{
    public class ApiClient
    {
        private static readonly HttpClient client = new HttpClient();
        private readonly ILogger<ApiClient> _logger;
        private readonly string _baseUrl;
        public ApiClient(ILogger<ApiClient> logger, IOptions<Settings> options)
        {
            _logger = logger;
            _baseUrl = options.Value.BaseUrl;
            if (string.IsNullOrWhiteSpace(_baseUrl))
            {
                _logger.LogCritical("BaseUrl не задан в конфигурации! Проверьте appsettings.json.");
                throw new ArgumentException("BaseUrl cannot be null or empty");
            }
        }
        public async Task SendHardwareInfo(HardwareInfo info)
        {
            try
            {
                _logger.LogInformation("Отправка данных на {Url}", _baseUrl);

                using HttpResponseMessage response = await client.PostAsJsonAsync(_baseUrl, info);

                if (!response.IsSuccessStatusCode)
                {
                    string errorResponse = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Сервер вернул ошибку {StatusCode}. Ответ сервера: {Details}",
                        (int)response.StatusCode, errorResponse);

                    response.EnsureSuccessStatusCode();
                }

                _logger.LogInformation("Данные успешно отправлены на сервер.");
            }
            catch (HttpRequestException e)
            {
                _logger.LogError(e, "Ошибка сети при отправке данных (API Client)");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Непредвиденная ошибка в ApiClient");
            }
        }
    }
}
