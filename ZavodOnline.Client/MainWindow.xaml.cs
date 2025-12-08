using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using ZavodOnline.Client.Models;

namespace ZavodOnline.Client
{
    public partial class MainWindow : Window
    {
        // Коллекция сообщений
        public ObservableCollection<MessageModel> Messages { get; } = new();

        public MainWindow()
        {
            InitializeComponent();

            // биндинги типа {Binding ...} будут смотреть на этот объект
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
        }

        // Общий метод отправки сообщения
        private void SendCurrentMessage()
        {
            string text = MessageTextBox.Text.Trim();

            if (string.IsNullOrEmpty(text))
                return;

            // Берём имя пользователя из верхнего поля
            string author = UserNameTextBox.Text.Trim();
            if (string.IsNullOrEmpty(author))
            {
                author = "Пользователь"; // запасной вариант
            }

            Messages.Add(new MessageModel
            {
                Author = author,
                Text = text,
                Timestamp = DateTime.Now,
                IsOwn = true
            });

            MessageTextBox.Clear();
            MessageTextBox.Focus();
        }


        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            SendCurrentMessage();
        }

        private void MessageTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // Главный Enter (Return) или NumPad Enter
            if (e.Key == Key.Return || e.Key == Key.Enter)
            {
                // Если зажат Shift — просто новая строка
                if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                {
                    return; // даём WPF вставить перевод строки
                }

                // Без Shift — отправляем сообщение
                SendCurrentMessage();

                // Помечаем, что клавишу обработали сами
                e.Handled = true;
            }
        }

    }
}
