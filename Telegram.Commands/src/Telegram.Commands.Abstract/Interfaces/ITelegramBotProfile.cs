namespace Telegram.Commands.Abstract.Interfaces
{
    public interface ITelegramBotProfile
    {
        string Key { get; set; }
        string BaseUrl { get; set; }
        string UpdateRoute { get; set; }
        string BotName { get; set; }
        //todo
        GroupMessageHandlingStrategy GroupMessageHandlingStrategy { get; set; }
    }

    public enum GroupMessageHandlingStrategy
    {
        Direct,
        Ignore,
        All
    }
}