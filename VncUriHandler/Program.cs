using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.Web;

if (args.Length > 0 && args[0].StartsWith("vnc://"))
{
    try
    {
        using var client = new HttpClient();
        client.Timeout = TimeSpan.FromSeconds(2);
        var encodedUri = WebUtility.UrlEncode(args[0]);
        await client.GetAsync($"http://localhost:5004/connect?uri={encodedUri}");
    }
    catch {  }
    return;
}
var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:5004");
var app = builder.Build();
string vncPath = app.Configuration["VncSettings:ViewerPath"] ?? @"C:\Program Files\TightVNC\tvnviewer.exe";
string vncControlPass = app.Configuration["VncSettings:DefaultControlPassword"] ?? "";
string vncViewPass = app.Configuration["VncSettings:DefaultViewPassword"] ?? "";
app.MapGet("/connect", (string uri) => {
    _ = LaunchVncLogic(uri, vncPath, vncControlPass, vncViewPass, app.Logger);
    return Results.Ok("Started");
});
await app.RunAsync();
async Task LaunchVncLogic(string uriString, string path, string controlPass, string viewPass, ILogger logger)
{
    try
    {
        var uri = new Uri(uriString);
        var query = HttpUtility.ParseQueryString(uri.Query);
        string ip = uri.Host;
        bool isReadOnly = query["mode"] != "control";
        string currentPass = isReadOnly ? viewPass : controlPass;
        string arguments = $"-password={currentPass} {ip}";
        var psi = new ProcessStartInfo
        {
            FileName = path,
            Arguments = arguments,
            UseShellExecute = true
        };
        var process = Process.Start(psi);
        if (process != null)
        {
            NativeMethods.AllowSetForegroundWindow(process.Id);
            for (int i = 0; i < 50; i++)
            {
                process.Refresh();
                IntPtr handle = process.MainWindowHandle;
                if (handle != IntPtr.Zero && NativeMethods.IsWindowVisible(handle))
                {
                    uint foregroundThread = NativeMethods.GetWindowThreadProcessId(NativeMethods.GetForegroundWindow(), IntPtr.Zero);
                    uint appThread = NativeMethods.GetCurrentThreadId();
                    uint targetThread = NativeMethods.GetWindowThreadProcessId(handle, IntPtr.Zero);
                    if (foregroundThread != targetThread)
                    {
                        NativeMethods.AttachThreadInput(foregroundThread, targetThread, true);
                        NativeMethods.BringWindowToTop(handle);
                        NativeMethods.ShowWindow(handle, NativeMethods.SW_SHOW);
                        NativeMethods.SetForegroundWindow(handle);
                        NativeMethods.AttachThreadInput(foregroundThread, targetThread, false);
                    }
                    else
                    {
                        NativeMethods.SetForegroundWindow(handle);
                    }
                    break;
                }
                await Task.Delay(100);
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Ошибка запуска VNC");
    }
}
static partial class NativeMethods
{
    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    [DllImport("user32.dll")]
    public static extern bool IsWindowVisible(IntPtr hWnd);
    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")]
    public static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr processId);
    [DllImport("user32.dll")]
    public static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);
    [DllImport("user32.dll")]
    public static extern bool BringWindowToTop(IntPtr hWnd);
    [DllImport("kernel32.dll")]
    public static extern uint GetCurrentThreadId();
    [DllImport("user32.dll")]
    public static extern bool AllowSetForegroundWindow(int dwProcessId);
    public const int SW_SHOW = 5;
}