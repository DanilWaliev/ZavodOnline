using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZavodOnline.Net.Models
{
    // Ответ на запрос аутентификации

    // Если пройдена - Success = true и отправляется Token
    // Иначе Success = false и отправляется текст ошибки
    public class AuthResponse
    {
        public bool Success { get; set; }
        public string Login { get; set; } = String.Empty;
        public string? Token { get; set; }
        public string? Error { get; set; }
    }
}
