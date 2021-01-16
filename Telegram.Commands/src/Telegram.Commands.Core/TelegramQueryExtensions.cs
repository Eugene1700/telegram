using System;
using System.Reflection;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Telegram.Commands.Core
{
    internal static class TelegramQueryExtensions
    {
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
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public static int GetId<T>(this T query)
        {
            return query.Switch(m => m.From.Id, cb => cb.From.Id);
        }
        
        public static long GetChatId<T>(this T query)
        {
            return query.Switch(m => m.Chat.Id, cb => cb.Message.Chat.Id);
        }
        
        public static long GetChatId<T>(this Update update)
        {
            switch (update.Type)
            {
                case UpdateType.Message:
                    return update.Message.GetChatId(); 
                case UpdateType.CallbackQuery:
                    return update.CallbackQuery.GetChatId();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static TResult Switch<T, TResult>(this T query, Func<Message, TResult> messageFunc,
            Func<CallbackQuery, TResult> callbackQueryFunc)
        {
            return query switch
            {
                Message message => messageFunc(message),
                CallbackQuery callbackQuery => callbackQueryFunc(callbackQuery),
                _ => throw new ArgumentOutOfRangeException()
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
                com = query.Substring(1).Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries)[0];
            }
            return com;
        }
    }
}