using System.IO;
using System.IO.Pipes;
using System.Windows;

namespace VncTrayApp
{
    public partial class App : Application
    {
        private StatusOverlay? _overlayWindow;
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            Task.Run(ListenToWorkerAsync);
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
                catch (TimeoutException)
                {
                    await Task.Delay(1000);
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() => _overlayWindow?.Hide());
                    await Task.Delay(2000);
                }
            }

        }


        private Task<string> ShowRequestWindow(string admin)
        {
            var tcs = new TaskCompletionSource<string>();

            Dispatcher.Invoke(() =>
            {
                var win = new RequestWindow(admin);
                win.Topmost = true;

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
