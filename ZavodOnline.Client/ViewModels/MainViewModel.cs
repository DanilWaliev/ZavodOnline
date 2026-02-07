using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using ZavodOnline.Client.Models;
using ZavodOnline.Net.Client;
using System.Windows;
using ZavodOnline.Net.Models;

namespace ZavodOnline.Client.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly IChatClient _chatClient;
        public string CurrentMessage { get; set; } = string.Empty;
        public ObservableCollection<MessageModel> Messages { get; } = new();

        private string _newMessageText = string.Empty;
        public string NewMessageText
        {
            get => _newMessageText;
            set
            {
                if (SetField(ref _newMessageText, value))
                {
                    UpdatePlaceholder();
                }
            }
        }

        private bool _isConnected;
        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                if (SetField(ref _isConnected, value))
                {
                    UpdatePlaceholder();
                }
            }
        }

        private string _placeholderText = string.Empty;
        public string PlaceholderText
        {
            get => _placeholderText;
            set => SetField(ref _placeholderText, value);
        }

        public ICommand SendMessageCommand { get; }

        public MainViewModel(string userName)
        {
            _chatClient = ((App)Application.Current).ChatClient;
            _chatClient.ChatMessageReceived += OnChatMessageReceived;

            SendMessageCommand = new AsyncRelayCommand(SendMessageAsync);
        }

        public MainViewModel() : this("Вы")
        {
        }

        private void UpdatePlaceholder()
        {
            PlaceholderText = IsConnected
                ? "Enter — отправить, Shift+Enter — новая строка"
                : "Нет подключения к серверу";
        }

        private bool CanSend()
        {
            return IsConnected && !string.IsNullOrWhiteSpace(NewMessageText);
        }

        private async Task SendMessageAsync()
        {
            if (!string.IsNullOrWhiteSpace(CurrentMessage))
            {
                await _chatClient.SendChatMessageAsync(CurrentMessage);
                CurrentMessage = string.Empty;
            }
        }

        private void OnChatMessageReceived(ChatMessage msg)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageModel msgModel = new MessageModel()
                {
                    Text = msg.Text,
                    Author = msg.AuthorLogin,
                    Timestamp = msg.Timestamp,
                    IsOwn = true,
                }
                ;
                Messages.Add(msgModel);
            });
        }
    }
}
