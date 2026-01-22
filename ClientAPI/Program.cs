using ClientAPI;
using HardwareAgent;
using HardwareShared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.Diagnostics;
using System.IO;

var exePath = Process.GetCurrentProcess().MainModule?.FileName;
var exeDir = Path.GetDirectoryName(exePath);
if (!string.IsNullOrEmpty(exeDir))
{
    Directory.SetCurrentDirectory(exeDir);
}

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(@"C:\ProgramData\Dallari\HardwareAgent\agent.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    var builder = Host.CreateApplicationBuilder(args);

    builder.Configuration.SetBasePath(exeDir ?? AppDomain.CurrentDomain.BaseDirectory);
    builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

    builder.Logging.ClearProviders();
    builder.Logging.AddSerilog();

    builder.Services.Configure<Settings>(
        builder.Configuration.GetSection("Settings"));

    builder.Services.AddSingleton<ClientAPI.HardwareAgent>();
    builder.Services.AddSingleton<ApiClient>();
    builder.Services.AddHostedService<Worker>();

    builder.Services.AddWindowsService(options =>
    {
        options.ServiceName = "HardwareAgentService";
    });

    var host = builder.Build();

    Log.Information("Служба HardwareAgentService запускается...");
    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Аварийное завершение службы при старте");
}
finally
{
    Log.CloseAndFlush();
}
