using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ZavodOnline.Net.Models;
using ZavodOnline.Net.TCP;

namespace ZavodOnline.Net.Protocol
{
    internal class ProtocolProcessor : IAsyncDisposable
    {
        readonly ITCPHandler tcpHandler;
        readonly List<byte> buffer = new();
        readonly object bufferLock = new();

        readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        public ProtocolProcessor(ITCPHandler tcpHandler)
        {
            this.tcpHandler = tcpHandler ?? throw new ArgumentNullException(nameof(tcpHandler));

            tcpHandler.DataReceived += OnDataReceived;
            tcpHandler.ConnectionStateChanged += OnConnectionStateChanged;
            tcpHandler.Error += OnTcpError;
        }

        public bool IsConnected => tcpHandler.IsConnected;

        public event Action<bool>? ConnectionStateChanged;
        public event Action<Exception>? Error;
        public event Action<AuthResponse>? AuthResponseReceived;
        public event Action<SystemMessage>? SystemMessageReceived;
        public event Action<UserData>? UserDataReceived;
        public event Action<ChatMessage>? ChatMessageReceived;

        public Task ConnectAsync(string host, int port) => tcpHandler.ConnectAsync(host, port);

        public Task DisconnectAsync() => tcpHandler.DisconnectAsync();

        // Отправка произвольной модели в виде фрейма протокола.
        public Task SendAsync<T>(MessageType type, T model)
        {
            byte[] payloadBytes;

            if (model is null || type == MessageType.Ping || type == MessageType.Pong)
            {
                // Для Ping/Pong можно не отправлять payload
                payloadBytes = Array.Empty<byte>();
            }
            else
            {
                string json = JsonSerializer.Serialize(model, jsonOptions);
                payloadBytes = Encoding.UTF8.GetBytes(json);
            }

            int length = 1 + payloadBytes.Length; // 1 байт тип + нагрузка
            byte[] frame = new byte[4 + length];

            // 4 байта длины
            WriteInt32BigEndian(frame, 0, length);

            // 1 байт типа
            frame[4] = (byte)type;

            // нагрузка
            if (payloadBytes.Length > 0)
            {
                Buffer.BlockCopy(payloadBytes, 0, frame, 5, payloadBytes.Length);
            }

            return tcpHandler.SendAsync(frame, 0, frame.Length);
        }

        void OnDataReceived(byte[] data)
        {
            lock (bufferLock)
            {
                buffer.AddRange(data);
                ProcessBuffer();
            }
        }

        void ProcessBuffer()
        {
            // В буфере может быть:
            // - меньше одного фрейма
            // - ровно один фрейм
            // - несколько фреймов подряд
            while (true)
            {
                if (buffer.Count < 4) return; // ждём минимум длину

                int length = ReadInt32BigEndian(buffer, 0);
                if (length <= 0)
                {
                    RaiseError(new InvalidOperationException($"Invalid frame length: {length}"));
                    // Очистка буфера, чтобы не зациклиться
                    buffer.Clear();
                    return;
                }

                int totalFrameSize = 4 + length;
                if (buffer.Count < totalFrameSize)
                {
                    // Ждём, пока придут остальные байты
                    return;
                }

                // У нас есть полный фрейм
                byte typeByte = buffer[4];
                int payloadLength = length - 1;

                byte[] payloadBytes = Array.Empty<byte>();
                if (payloadLength > 0)
                {
                    payloadBytes = new byte[payloadLength];
                    buffer.CopyTo(5, payloadBytes, 0, payloadLength);
                }

                // Удаляем обработанный фрейм из буфера
                buffer.RemoveRange(0, totalFrameSize);

                try
                {
                    HandleFrame((MessageType)typeByte, payloadBytes);
                }
                catch (Exception ex)
                {
                    // Ошибка обработки конкретного фрейма -
                    // сообщаем наверх, но продолжаем парсить остальные.
                    RaiseError(ex);
                }
            }
        }

        void HandleFrame(MessageType type, byte[] payloadBytes)
        {
            // Ping/Pong можно вообще не десериализовать
            if (type == MessageType.Ping || type == MessageType.Pong)
            {
                return;
            }

            string json = payloadBytes.Length > 0
                ? Encoding.UTF8.GetString(payloadBytes)
                : string.Empty;

            switch (type)
            {
                case MessageType.AuthResponse:
                    {
                        var msg = JsonSerializer.Deserialize<AuthResponse>(json, jsonOptions);
                        if (msg != null) AuthResponseReceived?.Invoke(msg);
                        break;
                    }
                case MessageType.SystemMessage:
                    {
                        var msg = JsonSerializer.Deserialize<SystemMessage>(json, jsonOptions);
                        if (msg != null) SystemMessageReceived?.Invoke(msg);
                        break;
                    }
                case MessageType.UserData:
                    {
                        var msg = JsonSerializer.Deserialize<UserData>(json, jsonOptions);
                        if (msg != null) UserDataReceived?.Invoke(msg);
                        break;
                    }
                case MessageType.ChatMessage:
                    {
                        var msg = JsonSerializer.Deserialize<ChatMessage>(json, jsonOptions);
                        if (msg != null) ChatMessageReceived?.Invoke(msg);
                        break;
                    }
                case MessageType.AuthRequest:
                    // Обычно клиент не получает AuthRequest от сервера,
                    // но если протокол когда-нибудь будет двусторонним - 
                    // здесь можно добавить обработку.
                    break;

                default:
                    throw new InvalidOperationException($"Unsupported message type: {type}");
            }
        }

        void OnConnectionStateChanged(bool isConnected)
        {
            ConnectionStateChanged?.Invoke(isConnected);
        }

        void OnTcpError(Exception ex)
        {
            RaiseError(ex);
        }

        void RaiseError(Exception ex)
        {
            Error?.Invoke(ex);
        }

        static void WriteInt32BigEndian(byte[] buffer, int offset, int value)
        {
            // Length у нас всегда >= 0, так что int достаточно.
            uint v = (uint)value;

            buffer[offset] = (byte)(v >> 24);
            buffer[offset + 1] = (byte)(v >> 16);
            buffer[offset + 2] = (byte)(v >> 8);
            buffer[offset + 3] = (byte)(v);
        }

        static int ReadInt32BigEndian(List<byte> buffer, int offset)
        {
            if (buffer.Count < offset + 4) throw new ArgumentOutOfRangeException(nameof(offset));

            uint b0 = buffer[offset];
            uint b1 = buffer[offset + 1];
            uint b2 = buffer[offset + 2];
            uint b3 = buffer[offset + 3];

            uint value = (b0 << 24) | (b1 << 16) | (b2 << 8) | b3;
            return (int)value;
        }

        public async ValueTask DisposeAsync()
        {
            tcpHandler.DataReceived -= OnDataReceived;
            tcpHandler.ConnectionStateChanged -= OnConnectionStateChanged;
            tcpHandler.Error -= OnTcpError;

            await tcpHandler.DisposeAsync();
        }
    }
}
