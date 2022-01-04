using Telegram.Bot;
using Telegram.Commands.Abstract.Interfaces;

namespace SimpleHandlers.Services
{
    public class TelegramBotProfile: ITelegramBotProfile
    {
        public string Key { get; set; }
        public string BaseUrl { get; set; }
        public string UpdateRoute => Settings.RouteToUpdate;
        public string BotName { get; set; }
        
        public GroupMessageHandlingStrategy GroupMessageHandlingStrategy => 
            GroupMessageHandlingStrategy.Direct;

        public string[] Swarms => null;
    }
}