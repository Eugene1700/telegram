using System;
using System.Linq;
using Telegram.Bot.Types;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Sessions
{
    public static class SessionCallbackQueryExtensions
    {
        public static CallbackData[] StoreSessionAndConvertToCallbackQuery<TNextCommand, TData>(this ICommandSessionManager commandSessionStore, long chatId, TData[] result, Func<TData, string> textProvider) where TNextCommand : ITelegramCommand<CallbackQuery>
        {
            var session =
                commandSessionStore.StoreSession<TNextCommand, TData, CallbackQuery>(chatId, result);

            return session.StoreObjects.Select(x => new CallbackData
            {
                Text = textProvider(x.Data),
                CallbackText = x.Id.ToString()
            }).ToArray();
        }
    }
}