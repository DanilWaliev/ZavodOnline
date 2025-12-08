using System;
using System.Collections.ObjectModel;
using System.Windows;
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

            // Приветственное системное сообщение (чужое)
            Messages.Add(new MessageModel
            {
                Author = "Система",
                Text = "Добро пожаловать в ЗаводОнлайн!",
                Timestamp = DateTime.Now,
                IsOwn = false
            });

            // Для примера ещё одно "чужое"
            Messages.Add(new MessageModel
            {
                Author = "Диспетчер смены",
                Text = "Отчёт по линии №3 нужен до 14:00.",
                Timestamp = DateTime.Now,
                IsOwn = false
            });
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string text = MessageTextBox.Text.Trim();

            if (string.IsNullOrEmpty(text))
                return;

            // Наше сообщение
            Messages.Add(new MessageModel
            {
                Author = "Вы",
                Text = text,
                Timestamp = DateTime.Now,
                IsOwn = true
            });

            MessageTextBox.Clear();
            MessageTextBox.Focus();
        }
    }
}
