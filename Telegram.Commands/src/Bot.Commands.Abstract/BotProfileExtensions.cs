using Bot.Commands.Abstract.Interfaces;

namespace Bot.Commands.Abstract
{
    public static class BotProfileExtensions
    {
        public static string GetFullUrl(this IBotProfile botProfile)
        {
            return $"{botProfile.BaseUrl}/{botProfile.UpdateRoute}";
        } 
    }
}