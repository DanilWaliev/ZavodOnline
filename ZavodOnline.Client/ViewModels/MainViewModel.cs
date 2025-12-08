using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using ZavodOnline.Client.Models;

namespace ZavodOnline.Client.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
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

        public MainViewModel()
        {
            Messages.Add(new MessageModel
            {
                Author = "Система",
                Text = "Добро пожаловать в ЗаводОнлайн!",
                Timestamp = DateTime.Now,
                IsOwn = false
            });

            Messages.Add(new MessageModel
            {
                Author = "Диспетчер смены",
                Text = "Отчёт по линии №3 нужен до 14:00.",
                Timestamp = DateTime.Now,
                IsOwn = false
            });

            // пока всегда считаем, что при запуске подключены
            IsConnected = true;
            UpdatePlaceholder();

            SendMessageCommand = new RelayCommand(
                _ => SendCurrentMessage(),
                _ => CanSend()
            );
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

        public void SendCurrentMessage()
        {
            if (!IsConnected)
                return;

            var text = NewMessageText.Trim();
            if (string.IsNullOrEmpty(text))
                return;

            const string author = "Вы";

            Messages.Add(new MessageModel
            {
                Author = author,
                Text = text,
                Timestamp = DateTime.Now,
                IsOwn = true
            });

            NewMessageText = string.Empty;
        }
    }
}
