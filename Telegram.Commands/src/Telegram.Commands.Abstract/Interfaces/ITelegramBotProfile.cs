namespace Telegram.Commands.Core
{
    public interface ITelegramBotProfile
    {
        string Key { get; set; }
        string BaseUrl { get; set; }
        string UpdateRoute { get; set; }
        
    }
}