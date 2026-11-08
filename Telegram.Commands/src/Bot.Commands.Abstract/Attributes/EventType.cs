using System;

namespace Bot.Commands.Abstract.Attributes
{

    public enum EventType
    {
        BotMemberAddedToChat,
        [Obsolete]
        MigrateToSuperGroup,
        ChatMemberAdded,
        ChatMemberLeft,
        SuccessfulPayment,
        ChatJoinRequest,
        BotStarted
    }
}
