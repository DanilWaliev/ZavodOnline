using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ZavodOnline.Net.TCP
{
    internal class TCPHandler : ITCPHandler
    {
        private TcpClient? tcpClient;
        private NetworkStream? stream;
        private CancellationTokenSource? cts;
        private Task? receiveTask;
        private readonly object sendLock = new object();

        public bool IsConnected { get; private set; }

        public event Action<bool>? ConnectionStateChanged;
        public event Action<byte[]>? DataReceived;
        public event Action<Exception>? Error;

        public async Task ConnectAsync(string host, int port)
        {
            if (IsConnected) return;

            tcpClient = new TcpClient();

            try
            {
                await tcpClient.ConnectAsync(host, port);

                stream = tcpClient.GetStream();
                cts = new CancellationTokenSource();

                receiveTask = Task.Run(() => ReceiveLoopAsync(cts.Token));

                IsConnected = true;
                ConnectionStateChanged?.Invoke(true);
            }
            catch (Exception ex)
            {
                Error?.Invoke(ex);
                Cleanup();
                return;
            }
        }

        public async Task DisconnectAsync()
        {
            if (!IsConnected)
                return;

            IsConnected = false;
            ConnectionStateChanged?.Invoke(false);

            try
            {
                cts?.Cancel();

                if (receiveTask != null) await receiveTask;
            }
            catch
            {
            }

            Cleanup();
        }

        public Task SendAsync(byte[] data, int offset = 0, int? count = null)
        {
            if (!IsConnected || stream == null)
                throw new InvalidOperationException("TCP connection is not established.");

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            int length = count ?? (data.Length - offset);
            if (offset < 0 || length < 0 || offset + length > data.Length)
                throw new ArgumentOutOfRangeException(nameof(offset), "Invalid offset/count for send buffer.");

            // NetworkStream не потокобезопасен, поэтому блокируем отправку
            lock (sendLock)
            {
                return stream.WriteAsync(data, offset, length);
            }
        }

        private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
        {
            if (stream == null)
                return;

            var buffer = new byte[4096];

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    int read = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                    if (read == 0)
                        throw new IOException("Remote host closed the connection.");

                    // Копируем только реально прочитанные байты
                    var chunk = new byte[read];
                    Buffer.BlockCopy(buffer, 0, chunk, 0, read);

                    DataReceived?.Invoke(chunk);
                }
            }
            catch (OperationCanceledException)
            {
                // Нормальное завершение по отмене
            }
            catch (Exception ex)
            {
                Error?.Invoke(ex);
                IsConnected = false;
                ConnectionStateChanged?.Invoke(false);
            }
        }

        private void Cleanup()
        {
            try
            {
                stream?.Close();
            }
            catch { }

            try
            {
                tcpClient?.Close();
            }
            catch { }

            stream = null;
            tcpClient = null;

            cts?.Dispose();
            cts = null;

            receiveTask = null;
        }

        public async ValueTask DisposeAsync()
        {
            await DisconnectAsync();
        }
    }
}
