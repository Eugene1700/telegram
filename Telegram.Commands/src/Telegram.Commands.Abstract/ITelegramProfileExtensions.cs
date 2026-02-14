using Telegram.Commands.Abstract.Interfaces;

namespace Telegram.Commands.Abstract
{
    public static class TelegramProfileExtensions
    {
        public static string GetFullUrl(this IBotProfile botProfile)
        {
            return $"{botProfile.BaseUrl}/{botProfile.UpdateRoute}";
        } 
    }
}