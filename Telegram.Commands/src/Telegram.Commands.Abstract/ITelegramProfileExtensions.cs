using Telegram.Commands.Abstract.Interfaces;

namespace Telegram.Commands.Abstract
{
    public static class TelegramProfileExtensions
    {
        public static string GetFullUrl(this ITelegramBotProfile telegramBotProfile)
        {
            return $"{telegramBotProfile.BaseUrl}/{telegramBotProfile.UpdateRoute}";
        } 
    }
}