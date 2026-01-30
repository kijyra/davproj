using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Windows;
namespace VncTrayApp
{
    public partial class App : Application
    {
        private StatusOverlay? _overlayWindow;
        private static Mutex? _mutex;
        private const string AppGuid = "VncTrayApp_SingleInstance_Mutex";
        private const string LocalCommandPipe = "VncTrayApp_Internal_Pipe";
        protected override void OnStartup(StartupEventArgs e)
        {
            _mutex = new Mutex(true, AppGuid, out bool isFirstInstance);

            if (!isFirstInstance)
            {
                if (e.Args.Length > 0)
                {
                    SendToMainInstance(e.Args[0]);
                }
                Current.Shutdown();
                return;
            }

            base.OnStartup(e);
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            Task.Run(ListenToWorkerAsync);
            Task.Run(ListenForInternalCommandsAsync);
            if (e.Args.Length > 0)
            {
                HandleProtocolUrl(e.Args[0]);
            }
        }
        private void HandleProtocolUrl(string url)
        {
            try
            {
                string target = url.Contains("://") ? url.Split("://")[1].TrimEnd('/') : url;

                if (url.StartsWith("rdpc:", StringComparison.OrdinalIgnoreCase))
                {
                    Process.Start(new ProcessStartInfo("mstsc.exe", $"/v:{target}") { UseShellExecute = true });
                }
                else if (url.StartsWith("rdps:", StringComparison.OrdinalIgnoreCase))
                {
                    Process.Start(new ProcessStartInfo("mstsc.exe", $"/v:{target} /shadow:1 /control /noConsentPrompt") { UseShellExecute = true });
                }
                else if (url.StartsWith("vnc:", StringComparison.OrdinalIgnoreCase))
                {
                    string vncPath = @"C:\Program Files\TightVNC\tvnviewer.exe";
                    if (File.Exists(vncPath))
                        Process.Start(vncPath, target);
                    else
                        MessageBox.Show("TightVNC Viewer не найден по пути: " + vncPath);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при обработке ссылки {url}: {ex.Message}");
            }
        }
        private async Task ListenForInternalCommandsAsync()
        {
            while (true)
            {
                try
                {
                    using var server = new NamedPipeServerStream(LocalCommandPipe, PipeDirection.In);
                    await server.WaitForConnectionAsync();
                    using var reader = new StreamReader(server);
                    var url = await reader.ReadLineAsync();
                    if (!string.IsNullOrEmpty(url))
                    {
                        Dispatcher.Invoke(() => HandleProtocolUrl(url));
                    }
                }
                catch { await Task.Delay(1000); }
            }
        }
        private void SendToMainInstance(string url)
        {
            try
            {
                using var client = new NamedPipeClientStream(".", LocalCommandPipe, PipeDirection.Out);
                client.Connect(1000);
                using var writer = new StreamWriter(client);
                writer.WriteLine(url);
            }
            catch {  }
        }
        private async Task ListenToWorkerAsync()
        {
            while (true)
            {
                try
                {
                    using var pipeClient = new NamedPipeClientStream(".", "VncControlPipe", PipeDirection.InOut, PipeOptions.Asynchronous);
                    await pipeClient.ConnectAsync(5000);
                    using var reader = new StreamReader(pipeClient);
                    using var writer = new StreamWriter(pipeClient) { AutoFlush = true };
                    var message = await reader.ReadLineAsync();
                    if (!string.IsNullOrEmpty(message))
                    {
                        var parts = message.Split('|');
                        string adminName = parts[0];
                        string result = await ShowRequestWindow(adminName);
                        await writer.WriteLineAsync(result);
                        if (result == "ALLOW")
                        {
                            var statusUpdate = await reader.ReadLineAsync();
                            if (statusUpdate == "SESSION_STARTED")
                            {
                                Dispatcher.Invoke(() => {
                                    if (_overlayWindow == null) _overlayWindow = new StatusOverlay();
                                    _overlayWindow.Show();
                                });
                                var endMessage = await reader.ReadLineAsync();
                                if (endMessage == "SESSION_ENDED")
                                {
                                    Dispatcher.Invoke(() => _overlayWindow?.Hide());
                                }
                            }
                        }
                    }
                }
                catch { await Task.Delay(2000); }
            }
        }
        private Task<string> ShowRequestWindow(string admin)
        {
            var tcs = new TaskCompletionSource<string>();
            Dispatcher.Invoke(() =>
            {
                var win = new RequestWindow(admin) { Topmost = true };
                win.OnResponse += (response) => {
                    tcs.SetResult(response);
                    win.Close();
                };
                win.Show();
            });
            return tcs.Task;
        }
    }
}
