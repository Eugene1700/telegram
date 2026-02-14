using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.Payments;
using Telegram.Commands.Abstract.Attributes;

namespace Telegram.Commands.Realization.Extensions
{
    internal static class TelegramQueryExtensions
    {
        private static readonly ChatType[] GroupChatTypes = [ChatType.Group, ChatType.Supergroup];

        public static ChatArea ToChatArea(this ChatType chatType)
        {
            return chatType switch
            {
                ChatType.Private => ChatArea.Private,
                ChatType.Group => ChatArea.Group,
                ChatType.Channel => ChatArea.Channel,
                ChatType.Supergroup => ChatArea.SuperGroup,
                _ => throw new ArgumentOutOfRangeException(nameof(chatType), chatType, null)
            };
        }
        
        public static bool IsGroupMessage(this ChatType chatType)
        {
            return GroupChatTypes.Contains(chatType);
        }

        private static TResult Switch<T, TResult>(this T query, Func<Message, TResult> messageFunc,
            Func<CallbackQuery, TResult> callbackQueryFunc, 
            Func<PreCheckoutQuery, TResult> preCheckoutQueryFunc,
                Func<ChatMemberUpdated, TResult> chatMemberUpdatedFunc,
            Func<ChatJoinRequest, TResult> chatJoinRequestFunc)
        {
            return query switch
            {
                Message message => messageFunc(message),
                CallbackQuery callbackQuery => callbackQueryFunc(callbackQuery),
                PreCheckoutQuery preCheckoutQuery => preCheckoutQueryFunc(preCheckoutQuery),
                ChatMemberUpdated chatMemberUpdatedQuery => chatMemberUpdatedFunc(chatMemberUpdatedQuery),
                ChatJoinRequest chatJoinRequest => chatJoinRequestFunc(chatJoinRequest),
                _ => throw new ArgumentOutOfRangeException(),
            };
        }
    }
}