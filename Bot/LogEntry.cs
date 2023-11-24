using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCTelegramBot.Bot
{
    public class LogEntry
    {
        public string Username { get; set; }
        public string ChatId { get; set; }
        public string Command { get; set; }
        public string Time { get; set; }
    }
}
