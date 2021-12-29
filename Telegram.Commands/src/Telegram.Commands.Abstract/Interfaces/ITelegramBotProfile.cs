namespace Telegram.Commands.Abstract.Interfaces
{
    public interface ITelegramBotProfile
    {
        string Key { get; }
        string BaseUrl { get; }
        string UpdateRoute { get; }
        string BotName { get; }
        GroupMessageHandlingStrategy GroupMessageHandlingStrategy { get; }
        string[] Swarms { get; }
    }

    public enum GroupMessageHandlingStrategy
    {
        Direct,
        Ignore,
        All
    }
}