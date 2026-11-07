namespace Bot.Commands.Abstract.Interfaces
{
    public interface IBotProfile
    {
        string Key { get; }
        string BaseUrl { get; }
        string UpdateRoute { get; }
        string BotName { get; }
        GroupMessageHandlingStrategy GroupMessageHandlingStrategy { get; }
        string[] Swarms { get; }
    }
}