/* using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace davproj.Filters
{
    public class ApiKeyAuthFilter : IAsyncAuthorizationFilter
    {
        private const string API_KEY_HEADER_NAME = "X-Api-Key";
        private readonly IConfiguration _configuration;

        public ApiKeyAuthFilter(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (!context.HttpContext.Request.Headers.TryGetValue(API_KEY_HEADER_NAME, out var extractedApiKey))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var apiKey = _configuration.GetValue<string>("ApiKeyJS");

            if (!apiKey.Equals(extractedApiKey))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

        }
    }
} */

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging; // Добавьте этот using

namespace davproj.Filters
{
    public class ApiKeyAuthFilter : IAsyncAuthorizationFilter
    {
        private const string API_KEY_HEADER_NAME = "X-Api-Key";
        private readonly IConfiguration _configuration;
        private readonly ILogger<ApiKeyAuthFilter> _logger; // Добавляем логгер

        // Обновляем конструктор для принятия ILogger
        public ApiKeyAuthFilter(IConfiguration configuration, ILogger<ApiKeyAuthFilter> logger)
        {
            _configuration = configuration;
            _logger = logger; // Инициализируем логгер
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (!context.HttpContext.Request.Headers.TryGetValue(API_KEY_HEADER_NAME, out var extractedApiKey))
            {
                _logger.LogWarning("Missing API Key header '{HeaderName}'", API_KEY_HEADER_NAME); // Логируем отсутствие заголовка
                context.Result = new UnauthorizedResult();
                return;
            }

            var expectedApiKey = _configuration.GetValue<string>("ApiKeyJS");

            // Добавляем логирование ожидаемого и полученного ключа
            _logger.LogInformation("Expected API Key: {ExpectedKey}", expectedApiKey);
            _logger.LogInformation("Received API Key: {ReceivedKey}", extractedApiKey);

            if (string.IsNullOrEmpty(expectedApiKey) || !expectedApiKey.Equals(extractedApiKey))
            {
                _logger.LogWarning("Invalid API Key provided."); // Логируем неверный ключ
                context.Result = new UnauthorizedResult();
                return;
            }

            _logger.LogInformation("API Key accepted.");
        }
    }
}

