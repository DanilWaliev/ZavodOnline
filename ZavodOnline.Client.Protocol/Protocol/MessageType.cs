using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZavodOnline.Net.Protocol
{
    public enum MessageType : byte
    {
        Ping = 0,
        Pong = 1,
        AuthRequest = 2,
        AuthResponse = 3,
        ChatMessage = 4, 
        SystemMessage = 5,
        UserData = 6,
    }
}
