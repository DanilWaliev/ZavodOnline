using System.Configuration;
using System.Data;
using System.Windows;
using ZavodOnline.Net.Client;

namespace ZavodOnline.Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public IChatClient ChatClient { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ChatClient = new ChatClient(); // конкретная реализация DLL
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            if (ChatClient.IsConnected)
                await ChatClient.DisconnectAsync();

            await ChatClient.DisposeAsync();
            base.OnExit(e);
        }
    }
}
