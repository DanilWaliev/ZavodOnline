using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ZavodOnline.Client.Models;

namespace ZavodOnline.Client
{
    public partial class MainWindow : Window
    {
        // Коллекция сообщений
        public ObservableCollection<MessageModel> Messages { get; } = new();

        // Флаг состояния подключения
        private bool _isConnected = false;

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

            // Приветственные сообщения
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

            // При запуске считаем, что нужно сразу подключаться к серверу.
            // Пока что эмуляция успешного подключения.
            SetConnectionState(true);
        }

        // Обновляем текст и видимость плейсхолдера.
        private void UpdatePlaceholder()
        {
            if (string.IsNullOrEmpty(MessageTextBox.Text))
            {
                PlaceholderTextBlock.Visibility = Visibility.Visible;
                PlaceholderTextBlock.Text = _isConnected
                    ? "Enter — отправить, Shift+Enter — новая строка"
                    : "Нет подключения к серверу";
            }
            else
            {
                PlaceholderTextBlock.Visibility = Visibility.Collapsed;
            }
        }

        // Устанавливаем состояние подключения и обновляем UI.
        private void SetConnectionState(bool connected)
        {
            _isConnected = connected;

            if (_isConnected)
            {
                // Подключено: поле активно и нормальное
                MessageTextBox.IsEnabled = true;
                MessageTextBox.Opacity = 1.0;
            }
            else
            {
                // Не подключено: поле заблокировано и притемнено
                MessageTextBox.IsEnabled = false;
                MessageTextBox.Opacity = 0.6;

                // На всякий случай очищаем, чтобы плейсхолдер был виден
                MessageTextBox.Text = string.Empty;
            }

            UpdatePlaceholder();
        }

        // Общий метод отправки сообщения
        private void SendCurrentMessage()
        {
            // Если нет подключения — не даём отправлять
            if (!_isConnected)
            {
                MessageBox.Show("Нет подключения к серверу.", "ЗаводОнлайн",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string text = MessageTextBox.Text.Trim();

            if (string.IsNullOrEmpty(text))
                return;

            const string author = "Вы"; // позже возьмём из аутентификации

            Messages.Add(new MessageModel
            {
                Author = author,
                Text = text,
                Timestamp = DateTime.Now,
                IsOwn = true
            });

            MessageTextBox.Clear();
            MessageTextBox.Focus();
            UpdatePlaceholder();
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            SendCurrentMessage();
        }

        private void MessageTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return || e.Key == Key.Enter)
            {
                if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                {
                    // Shift+Enter — просто новая строка
                    return;
                }

                SendCurrentMessage();
                e.Handled = true;
            }
        }

        private void MessageTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdatePlaceholder();
        }
    }
}
