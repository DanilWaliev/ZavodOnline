using System.Windows;
using ZavodOnline.Client.ViewModels;

namespace ZavodOnline.Client
{
    public partial class LoginWindow : Window
    {
        private readonly LoginViewModel _viewModel;

        public LoginWindow()
        {
            InitializeComponent();

            _viewModel = new LoginViewModel();
            _viewModel.LoginSucceeded += OnLoginSucceeded;

            DataContext = _viewModel;
        }

        private void OnLoginSucceeded()
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
            Close();
        }

        private void OnLoginClicked(object sender, RoutedEventArgs e)
        {
            _viewModel.PasswordHash = PasswordBox.Password;
        }

        // обработчик кнопки "Регистрация"
        private void OnRegisterClicked(object sender, RoutedEventArgs e)
        {
            var registerWindow = new RegisterWindow();
            registerWindow.Owner = this;
            registerWindow.ShowDialog();
        }
    }
}
