using System;
using System.Windows;
using System.Windows.Controls;

namespace VncTrayApp
{
    public partial class RequestWindow : Window
    {
        public event Action<string>? OnResponse;

        public RequestWindow(string admin)
        {
            InitializeComponent();

            InfoText.Text = $"Администратор {admin} запрашивает удаленное управление вашим ПК. Разрешить?";
        }

        private void Allow_Click(object sender, RoutedEventArgs e)
        {
            OnResponse?.Invoke("ALLOW");
        }

        private void Deny_Click(object sender, RoutedEventArgs e)
        {
            OnResponse?.Invoke("DENY");
        }
    }
}
