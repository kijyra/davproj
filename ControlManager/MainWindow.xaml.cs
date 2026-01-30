using ControlManager.DTO;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Net.Http;
using System.Net.Http.Json;
using ControlManager.DTO;

namespace ControlManager
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async Task LoadWorkstations()
        {
            var handler = new HttpClientHandler() { UseDefaultCredentials = true };

            using HttpClient client = new HttpClient(handler);
            try
            {
                string url = "http://10.0.0.70/VNC/GetCustomWorkstations";
                var stations = await client.GetFromJsonAsync<List<WorkstationDto>>(url);
                WorkstationsGrid.ItemsSource = stations;
            }
            catch (HttpRequestException httpEx)
            {
                System.Windows.MessageBox.Show($"Ошибка сети: {httpEx.Message}\nПроверьте доступность сервера 10.0.0.70");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка загрузки: {ex.Message}");
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _ = LoadWorkstations();
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.Tag is string url)
            {
                try
                {
                    var uri = new Uri(url);
                    string protocol = uri.Scheme;
                    string host = uri.Host;
                    var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                    bool isViewOnly = query["mode"] == "view";

                    var connWindow = new Connection(host, isViewOnly, protocol);
                    connWindow.Show();
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Ошибка запуска: {ex.Message}");
                }
            }
        }

    }
}
