using global::Telegram.Bot;
using Bot.Commands.Abstract.Interfaces;

namespace SimpleHandlers.Services
{
    public class BotProfile: IBotProfile
    {
        public string Key { get; set; }
        public string BaseUrl { get; set; }
        public string UpdateRoute { get; set; } = Settings.RouteToUpdate;
        public string BotName { get; set; }
        public string Platform { get; set; }
        
        public GroupMessageHandlingStrategy GroupMessageHandlingStrategy => 
            GroupMessageHandlingStrategy.Direct;

        public string[] Swarms { get; set; }
    }
}
