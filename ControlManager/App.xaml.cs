using System;
using System.Windows;
using System.Web;

namespace ControlManager
{
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            if (e.Args.Length > 0)
            {
                string rawUrl = e.Args[0];
                try
                {
                    var uri = new Uri(rawUrl);
                    string protocol = uri.Scheme.ToLower();
                    string host = uri.Host;
                    var query = HttpUtility.ParseQueryString(uri.Query);
                    string mode = query["mode"] ?? "full";
                    bool isViewOnly = mode.ToLower() == "view";
                    HandleConnection(protocol, host, isViewOnly);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Ошибка разбора ссылки: {ex.Message}");
                    Shutdown();
                }
            }
            else
            {
                new MainWindow().Show();
            }
        }

        private void HandleConnection(string protocol, string host, bool isViewOnly)
        {
            var connWin = new Connection(host, isViewOnly, protocol == "rdpc" ? "rdp" : "vnc");
            connWin.Show();
        }

    }
}
