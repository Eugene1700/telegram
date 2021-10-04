using System;
using System.Linq;
using System.Reflection;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.Payments;
using Telegram.Commands.Abstract;
using Telegram.Commands.Abstract.Interfaces;

namespace Telegram.Commands.Core
{
    internal static class TelegramQueryExtensions
    {
        private static readonly ChatType[] GroupChatTypes = new [] {ChatType.Group, ChatType.Supergroup};

        public static Message AsMessage<T>(this T query)
        {
            return query switch
            {
                Message message => message,
                CallbackQuery callbackQuery => callbackQuery.Message,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public static string GetData<T>(this T query)
        {
            return query switch
            {
                Message message => message.Text,
                CallbackQuery callbackQuery => callbackQuery.Data,
                PreCheckoutQuery preCheckoutQuery => preCheckoutQuery.InvoicePayload,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public static long GetFromId<T>(this T query)
        {
            return query.Switch(m => m.From.Id, cb => cb.From.Id,
                prq => prq.From.Id,
                cm => cm.From.Id);
        }
        
        public static long GetChatId<T>(this T query)
        {
            return query.Switch(m => m.Chat.Id, cb => cb.Message.Chat.Id,
                prq => prq.From.Id,
                cm => cm.Chat.Id);
        }
        
        public static ChatType GetChatType<T>(this T query)
        {
            return query.Switch(m => m.Chat.Type, cb => cb.Message.Chat.Type,
                prq => ChatType.Private,
                cm => cm.Chat.Type);
        }
        
        public static bool IsGroupMessage<T>(this T query)
        {
            return GroupChatTypes.Contains(query.GetChatType());
        }
        
        public static long GetChatId<T>(this Update update)
        {
            switch (update.Type)
            {
                case UpdateType.Message:
                    return update.Message.GetChatId(); 
                case UpdateType.CallbackQuery:
                    return update.CallbackQuery.GetChatId();
                case UpdateType.PreCheckoutQuery:
                    return update.PreCheckoutQuery.GetChatId();
                case UpdateType.MyChatMember:
                    return update.MyChatMember.GetChatId();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static TResult Switch<T, TResult>(this T query, Func<Message, TResult> messageFunc,
            Func<CallbackQuery, TResult> callbackQueryFunc, 
            Func<PreCheckoutQuery, TResult> preCheckoutQueryFunc,
                Func<ChatMemberUpdated, TResult> chatMemberUpdatedFunc)
        {
            return query switch
            {
                Message message => messageFunc(message),
                CallbackQuery callbackQuery => callbackQueryFunc(callbackQuery),
                PreCheckoutQuery preCheckoutQuery => preCheckoutQueryFunc(preCheckoutQuery),
                ChatMemberUpdated chatMemberUpdatedQuery => chatMemberUpdatedFunc(chatMemberUpdatedQuery),
                _ => throw new ArgumentOutOfRangeException(),
            };
        }

        public static bool MatchCommand(this ITelegramCommandDescriptor descriptor, string query)
        {
            if (string.IsNullOrEmpty(query)) return false;

            var com = ExtractCommand(query);
            return com == descriptor.Name;
        }

        public static string ExtractCommand(string query)
        {
            string com = "";
            if (query[0] == '/')
            {
                var botNameStart = query.IndexOf("@");
                var len = botNameStart == -1 ? query.Length - 1 : botNameStart - 1;
                com = query.Substring(1, len).Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries)[0];
            }
            return com;
        }

        public static bool MatchReaction(this ITelegramCommandDescriptor descriptor, 
            ITelegramCommandDescriptor currentCommandInfo)
        {
            return descriptor.Reaction != null && descriptor.Reaction.Any(x =>
                TelegramCommandExtensions.GetCommandInfo(x).Name == currentCommandInfo.Name);
        }
    }
}