using System.Windows;
namespace VncTrayApp
{
    public partial class RequestWindow : Window
    {
        public event Action<string>? OnResponse;
        public RequestWindow(string admin)
        {
            InitializeComponent();
            textBlock.Text = $"Администратор {admin} запрашивает доступ к управлению.";
            this.MouseDown += (s, e) => {
                if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed) this.DragMove();
            };
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
