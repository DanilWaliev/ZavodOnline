using System.Windows;

namespace ZavodOnline.Client
{
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
        }

        private void OnRegisterClicked(object sender, RoutedEventArgs e)
        {
            // пока без логики — просто закрываем окно
            MessageBox.Show(
                "Регистрация пока не реализована",
                "Информация",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            Close();
        }
    }
}
