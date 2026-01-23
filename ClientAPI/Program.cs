using ClientAPI;
using HardwareAgent;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Serilog;
using System.Diagnostics;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Claims;
using System.Security.Principal;
using System.Text.Encodings.Web;

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
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseWindowsService();

    builder.WebHost.ConfigureKestrel(options => {
        options.Listen(System.Net.IPAddress.Any, 5005);
    });

    builder.Configuration.SetBasePath(exeDir ?? AppDomain.CurrentDomain.BaseDirectory);
    builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

    builder.Logging.ClearProviders();
    builder.Logging.AddSerilog();

    builder.Services.Configure<Settings>(builder.Configuration.GetSection("Settings"));

    builder.Services.AddSingleton<ClientAPI.HardwareAgent>();
    builder.Services.AddSingleton<ApiClient>();
    builder.Services.AddSingleton<VNCServiceManager>();
    builder.Services.AddHostedService<Worker>();

    builder.Services.AddWindowsService(options =>
    {
        options.ServiceName = "HardwareAgentService";
    });

    builder.Services.AddAuthentication("FakeScheme")
        .AddCookie("FakeScheme", options => { });

    builder.Services.AddAuthorization(options =>
    {
        options.DefaultPolicy = new AuthorizationPolicyBuilder().RequireAssertion(_ => true).Build();
    });

    var app = builder.Build();

    app.MapPost("/vnc/request", async (VncRequest request, VNCServiceManager vncManager) =>
    {
        if (vncManager.IsVncActive())
            return Results.Conflict("Сессия уже активна.");

        if (!request.IsFullControl)
        {
            vncManager.SetupAndStart(false);
            return Results.Ok("View-only mode started.");
        }

        var result = await vncManager.RequestUserPermission(request);

        return result switch
        {
            "ALLOW" => Results.Ok("User allowed access."),
            "DENY" => Results.Forbid(),
            _ => Results.StatusCode(500)
        };
    });


    Log.Information("Служба HardwareAgentService с API запускается...");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Аварийное завершение службы при старте");
}
finally
{
    Log.CloseAndFlush();
}

public class FakeAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public FakeAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
        : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[] { new Claim(ClaimTypes.Name, "FakeUser") };
        var identity = new ClaimsIdentity(claims, "FakeScheme");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "FakeScheme");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
public record VncRequest(string AdminName, bool IsFullControl);