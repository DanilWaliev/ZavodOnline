using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZavodOnline.Net.TCP
{
    internal interface ITCPHandler : IAsyncDisposable
    {
        bool IsConnected { get; }

        event Action<bool>? ConnectionStateChanged;
        event Action<byte[]>? DataReceived;
        event Action<Exception>? Error;

        Task ConnectAsync(string host, int port);
        Task DisconnectAsync();

        Task SendAsync(byte[] data, int offset = 0, int? count = null);
    }
}
