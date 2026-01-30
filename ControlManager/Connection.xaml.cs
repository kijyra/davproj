using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows;
using System.Windows.Forms.Integration;
using VncSharpCore;

namespace ControlManager
{
    public partial class Connection : Window
    {
        private RemoteDesktop vncClient;

        private string _host;
        private bool _viewOnly;
        private string _protocol;

        public Connection(string host, bool viewOnly, string protocol = "vnc")
        {
            InitializeComponent();
            _host = host;
            _viewOnly = viewOnly;
            _protocol = protocol.ToLower();
            _ = StartConnection();
            this.Title = $"Подключение к {_host} ({_protocol}) " + (_viewOnly ? "(Просмотр)" : "(Управление)");
            if (_protocol == "vnc")
            {
                InitVnc();
            }
        }
        private async Task StartConnection()
        {
            if (_protocol == "vnc")
            {
                bool isAllowed = await RequestVncPermission();

                if (isAllowed)
                {
                    InitVnc();
                }
                else
                {
                    this.Close();
                }
            }
            else
            {
                
            }
        }
        private async Task<bool> RequestVncPermission()
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(120);

            var vncRequest = new
            {
                AdminName = Environment.UserName,
                IsFullControl = !_viewOnly
            };

            try
            {
                var response = await client.PostAsJsonAsync($"http://{_host}:5005/vnc/request", vncRequest);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    System.Windows.MessageBox.Show("Пользователь отклонил запрос на подключение.", "Отказ");
                    return false;
                }

                System.Windows.MessageBox.Show($"Агент вернул ошибку: {response.StatusCode}");
                return false;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Не удалось связаться с агентом на {_host}:5005.\nУбедитесь, что ПК включен и агент запущен.\n\nДетали: {ex.Message}", "Ошибка связи");
                return false;
            }
        }

        private void InitVnc()
        {
            vncClient = new VncSharpCore.RemoteDesktop();
            HostContainer.Child = vncClient;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_protocol == "vnc")
                {
                    vncClient.Connect(_host, 5900, _viewOnly);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (vncClient != null && vncClient.IsConnected)
                    vncClient.Disconnect();
            }
            catch { }

            base.OnClosing(e);
        }
    }
}
