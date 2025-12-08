using System.Windows;

namespace ZavodOnline.Client
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Приветственное сообщение
            MessagesListBox.Items.Add("[Система] Добро пожаловать в ЗаводОнлайн!");
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            // Берём текст из поля ввода
            string text = MessageTextBox.Text.Trim();

            if (string.IsNullOrEmpty(text))
            {
                return; // ничего не вводили — ничего не делаем
            }

            // Формируем строку сообщения
            string message = $"[Вы] {text}";

            // Добавляем в список сообщений
            MessagesListBox.Items.Add(message);

            // Очищаем поле ввода и ставим туда курсор
            MessageTextBox.Clear();
            MessageTextBox.Focus();
        }
    }
}
