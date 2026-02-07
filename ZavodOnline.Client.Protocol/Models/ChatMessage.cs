using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ZavodOnline.Net.Models
{
    // Сообщение в чате
    public class ChatMessage
    {
        // Логин автора
        public string AuthorLogin { get; set; } = String.Empty;
        // Текст сообщения 
        public string Text { get; set; } = String.Empty;
        // Время отправки сообщения
        public DateTime Timestamp { get; set; }
    }
}
