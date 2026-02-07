using System.Collections.Concurrent;
using ZavodOnline.Net.Models;
using ZavodOnline.Net.Protocol;
using ZavodOnline.Net.TCP;

namespace ZavodOnline.Net.Client;

public sealed class ChatClient : IChatClient
{
    readonly ProtocolProcessor protocolProcessor;
    readonly TCPHandler tcpHandler;

    readonly ConcurrentDictionary<string, UserData> userCache = new();

    string? login;
    string? token;

    public ChatClient()
    {
        tcpHandler = new TCPHandler();
        protocolProcessor = new ProtocolProcessor(tcpHandler);

        protocolProcessor.ConnectionStateChanged += connected =>
        {
            ConnectionStateChanged?.Invoke(connected);
        };

        protocolProcessor.AuthResponseReceived += OnAuthResponseReceived;
        protocolProcessor.SystemMessageReceived += msg => SystemMessageReceived?.Invoke(msg);
        protocolProcessor.UserDataReceived += OnUserDataReceived;
        protocolProcessor.ChatMessageReceived += msg => ChatMessageReceived?.Invoke(msg);
        protocolProcessor.Error += ex =>
        {
            // Можно дополнительно прокидывать как SystemMessage или отдельным событием.
            // Сейчас просто не глушим — пусть верхний уровень решает.
        };
    }

    public bool IsConnected => protocolProcessor.IsConnected;

    public string? Login => login;
    public string? Token => token;

    public event Action<bool>? ConnectionStateChanged;

    public event Action<AuthResponse>? AuthResponseReceived;
    public event Action<SystemMessage>? SystemMessageReceived;
    public event Action<UserData>? UserDataReceived;
    public event Action<ChatMessage>? ChatMessageReceived;

    public Task ConnectAsync(string host, int port) => protocolProcessor.ConnectAsync(host, port);

    public Task DisconnectAsync() => protocolProcessor.DisconnectAsync();

    public Task LoginWithPasswordAsync(string login, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(login)) throw new ArgumentException("Login is required.", nameof(login));
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("PasswordHash is required.", nameof(passwordHash));

        this.login = login;

        var request = new AuthRequest
        {
            Login = login,
            PasswordHash = passwordHash,
            Token = null
        };

        return protocolProcessor.SendAsync(MessageType.AuthRequest, request);
    }

    public Task LoginWithTokenAsync(string login, string token)
    {
        if (string.IsNullOrWhiteSpace(login)) throw new ArgumentException("Login is required.", nameof(login));
        if (string.IsNullOrWhiteSpace(token)) throw new ArgumentException("Token is required.", nameof(token));

        this.login = login;
        this.token = token;

        var request = new AuthRequest
        {
            Login = login,
            PasswordHash = null,
            Token = token
        };

        return protocolProcessor.SendAsync(MessageType.AuthRequest, request);
    }

    public Task SendChatMessageAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return Task.CompletedTask;

        // По договорённости: токен не кладём в каждое сообщение.
        // Автор сервер может поставить по сессии, поэтому можно оставить пустым.
        var msg = new ChatMessage
        {
            AuthorLogin = string.Empty,
            Text = text,
            Timestamp = DateTime.UtcNow
        };

        return protocolProcessor.SendAsync(MessageType.ChatMessage, msg);
    }

    public bool TryGetUserData(string login, out UserData userData)
    {
        if (string.IsNullOrWhiteSpace(login))
        {
            userData = default!;
            return false;
        }

        return userCache.TryGetValue(login, out userData!);
    }

    void OnUserDataReceived(UserData userData)
    {
        if (!string.IsNullOrWhiteSpace(userData.Login)) userCache[userData.Login] = userData;
        UserDataReceived?.Invoke(userData);
    }

    void OnAuthResponseReceived(AuthResponse response)
    {
        if (response.Success)
        {
            login = response.Login;

            // Сервер присылает токен при успешной аутентификации
            if (!string.IsNullOrWhiteSpace(response.Token)) token = response.Token;
        }

        AuthResponseReceived?.Invoke(response);
    }

    public async ValueTask DisposeAsync()
    {
        await protocolProcessor.DisposeAsync();
        await tcpHandler.DisposeAsync();
    }
}
