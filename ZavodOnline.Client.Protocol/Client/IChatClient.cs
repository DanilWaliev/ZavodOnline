using ZavodOnline.Net.Models;

namespace ZavodOnline.Net.Client;

public interface IChatClient : IAsyncDisposable
{
    bool IsConnected { get; }
    string? Login { get; }
    string? Token { get; }

    event Action<bool>? ConnectionStateChanged;

    event Action<AuthResponse>? AuthResponseReceived;
    event Action<SystemMessage>? SystemMessageReceived;
    event Action<UserData>? UserDataReceived;
    event Action<ChatMessage>? ChatMessageReceived;

    Task ConnectAsync(string host, int port);
    Task DisconnectAsync();

    Task LoginWithPasswordAsync(string login, string passwordHash);
    Task LoginWithTokenAsync(string login, string token);

    Task SendChatMessageAsync(string text);

    bool TryGetUserData(string login, out UserData userData);
}
