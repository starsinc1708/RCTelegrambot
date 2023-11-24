using System;

namespace RCTelegramBot.Bot
{
    [AttributeUsage(AttributeTargets.Method)]
    internal class BotCommandAttribute : Attribute
    {
        public string Command { get; }

        public BotCommandAttribute(string command)
        {
            Command = command;
        }
    }
}
