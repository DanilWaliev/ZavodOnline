using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZavodOnline.Net.Models
{
    // Запрос на аутентификацию

    // Когда у пользователя нет сохраненного входа в памяти - отправляется логин и пароль
    // Когда есть - отправляется логин и токен

    public class AuthRequest
    {
        public string Login { get; set; } = String.Empty;
        public string? PasswordHash { get; set; }
        public string? Token { get; set; }
    }
}
