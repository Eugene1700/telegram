namespace Telegram.Commands.Abstract
{
    public class EventAttribute : CommandAttribute
    {
        public EventType Type { get; set; }
        public int Order { get; set; }
    }

    public enum EventType
    {
        BotMemberAddedToChat,
        MigrateToSuperGroup,
        ChatMemberAdded
    }
}