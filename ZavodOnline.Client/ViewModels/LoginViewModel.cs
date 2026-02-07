using System;
using System.Windows.Input;
using ZavodOnline.Net.Client;
using ZavodOnline.Net.Models;
using ZavodOnline.Client;
using System.Windows; // для RelayCommand

namespace ZavodOnline.Client.ViewModels
{
    public class LoginSucceededEventArgs : EventArgs
    {
        public LoginSucceededEventArgs(string userName)
        {
            UserName = userName;
        }

        public string UserName { get; }
    }

    public class LoginViewModel : BaseViewModel
    {
        private readonly IChatClient _chatClient;
        public string Login { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? ErrorMessage { get; private set; } = string.Empty;

        public ICommand LoginCommand { get; }
        public event Action? LoginSucceeded;

        public LoginViewModel()
        {
            _chatClient = ((App)Application.Current).ChatClient;
            _chatClient.AuthResponseReceived += OnAuthResponseReceived;
            LoginCommand = new AsyncRelayCommand(LoginAsync);
        }

        private async Task LoginAsync()
        {
            try
            {
                if (!_chatClient.IsConnected) await _chatClient.ConnectAsync("localhost", 5000);

                await _chatClient.LoginWithPasswordAsync(Login, PasswordHash);
            }
            catch(Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        private void OnAuthResponseReceived(AuthResponse response)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (response.Success)
                {
                    _chatClient.AuthResponseReceived -= OnAuthResponseReceived;
                    LoginSucceeded?.Invoke();
                }
                else
                {
                    ErrorMessage = response.Error;
                }
            });
        }
    }
}
