using System.Windows;

namespace ZavodOnline.Client
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string userName = UserNameTextBox.Text.Trim();

            if (string.IsNullOrEmpty(userName))
            {
                MessageBox.Show("Введите имя пользователя.", "ЗаводОнлайн",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Открываем главное окно, передаём имя
            var mainWindow = new MainWindow();
            mainWindow.Show();

            // Закрываем окно логина
            this.Close();
        }
    }
}
