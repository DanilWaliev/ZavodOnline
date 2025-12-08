using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZavodOnline.Client.Models
{
    public class MessageModel
    {
        public string Author { get; set; } = "";
        public string Text { get; set; } = "";
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public bool IsOwn { get; set; }
    }
}
