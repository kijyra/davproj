using HardwareAgent;
using HardwareShared;
using Microsoft.Extensions.Options;

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
                var options = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                };
                string json = System.Text.Json.JsonSerializer.Serialize(info, options);
                _logger.LogInformation("Отправка данных на {Url}", _baseUrl);
                using StringContent content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                using HttpResponseMessage response = await client.PostAsync(_baseUrl, content);
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
