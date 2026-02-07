using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZavodOnline.Net.Models
{
    // Данные о пользователе
    public class UserData
    {
        public string Login { get; set; } = String.Empty;
        public string Fullname { get; set; } = String.Empty;
        public string Position { get; set; } = String.Empty;
    }
}
