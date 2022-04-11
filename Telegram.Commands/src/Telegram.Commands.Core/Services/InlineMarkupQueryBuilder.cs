using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Abstract.Interfaces.Commands;
using Telegram.Commands.Core.Exceptions;

namespace Telegram.Commands.Core.Services
{
    public class InlineMarkupQueryBuilder
    {
        private readonly List<InlineKeyboardButton[]> _buttons;

        public InlineMarkupQueryBuilder()
        {
            _buttons = new List<InlineKeyboardButton[]>();
        }

        public InlineMarkupQueryBuilder AddInlineKeyboardButtons<TCommand, TQueryData>(CallbackData<TQueryData>[] callbackQueries)
            where TCommand : IDataQueryTelegramCommand<CallbackQuery, TQueryData>
        {
            var commandDescriptor = TelegramCommandExtensions.GetCommandInfo<TCommand, CallbackQuery>();
            return AddInlineKeyBoardButtonsInternal(callbackQueries, commandDescriptor);
        }
        
        public InlineMarkupQueryBuilder AddInlineKeyboardButtons<TCommand, TQueryData, TSessionObject>(CallbackData<TQueryData>[] callbackQueries)
            where TCommand : ISessionTelegramCommand<CallbackQuery, TSessionObject>
        {
            var commandDescriptor = TelegramCommandExtensions.GetCommandInfo<TCommand, CallbackQuery, TSessionObject>();
            return AddInlineKeyBoardButtonsInternal(callbackQueries, commandDescriptor);
        }

        public InlineMarkupQueryBuilder AddInlineKeyboardButton<TCommand, TQueryData>(CallbackData<TQueryData> callbackData)
            where TCommand : IQueryTelegramCommand<CallbackQuery>
        {
            return AddInlineKeyboardButtons<TCommand, TQueryData>(new[] {callbackData});
        }
        
        public InlineMarkupQueryBuilder AddInlineKeyboardButton<TCommand, TQueryData, TSessionObject>(CallbackData<TQueryData> callbackData)
            where TCommand : ISessionTelegramCommand<CallbackQuery, TSessionObject>
        {
            return AddInlineKeyboardButtons<TCommand, TQueryData, TSessionObject>(new[] {callbackData});
        }

        public InlineMarkupQueryBuilder AddInlineKeyboardButton(CallbackDataObj[] callbackQueries)
        {
            if (callbackQueries.Any(x => x.CallbackText.Length > 64 && x.CallbackMode == CallbackMode.CallbackData))
                throw new TelegramCommandsInternalException("Сallback message is too long");
            
            _buttons.AddRange(callbackQueries.Select(
                x => new[]
                {
                    CreateNewInlineKeyboardButton(x)
                }
            ).ToArray());
            return this;
        }

        public InlineMarkupQueryBuilder AddInlineKeyboardButton(CallbackDataObj callbackData)
        {
            return AddInlineKeyboardButton(new[] {callbackData});
        }

        public InlineMarkupQueryBuilder InlineKeyboardButtonsRow(CallbackDataWithCommand[] callbackData)
        {
            _buttons.Add(callbackData.Select(CreateNewInlineKeyboardButton).ToArray());
            return this;
        }

        public IReplyMarkup GetResult()
        {
            return new InlineKeyboardMarkup(_buttons);
        }

        private InlineKeyboardButton CreateNewInlineKeyboardButton<T>(CallbackDataWithCommand<T> callbackDataWithCommand)
        {
            return CreateNewInlineKeyboardButton(callbackDataWithCommand, callbackDataWithCommand.CommandDescriptor);
        }

        private static InlineKeyboardButton CreateNewInlineKeyboardButton(CallbackDataObj x,
            ITelegramCommandDescriptor commandDescriptor = null)
        {

            switch (x.CallbackMode)
            {
                case CallbackMode.CallbackData:
                    var callbackData = commandDescriptor == null
                        ? x.Data.Serialize()
                        : $"/{commandDescriptor.Name}?{x.Data.Serialize()}";
                    return InlineKeyboardButton.WithCallbackData(x.Text, callbackData);
                case CallbackMode.InlineCurrent:
                    var strCurrent = GetString(x);
                    return InlineKeyboardButton.WithSwitchInlineQueryCurrentChat(x.Text, strCurrent);
                case CallbackMode.Inline:
                    var strInline = GetString(x);
                    return InlineKeyboardButton.WithSwitchInlineQuery(x.Text, strInline);
                case CallbackMode.Url:
                    //todo validate url
                    var url = GetString(x);
                    return InlineKeyboardButton.WithUrl(x.Text, url);
                case CallbackMode.Payment:
                    return InlineKeyboardButton.WithPayment(x.Text);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static string GetString(CallbackDataObj x)
        {
            if (!(x.Data is string str))
                throw new TelegramCommandsInternalException("Invalid type for mode");
            return str;
        }

        private InlineMarkupQueryBuilder AddInlineKeyBoardButtonsInternal(
            CallbackDataObj[] callbackQueries, ITelegramCommandDescriptor commandDescriptor)
        {
            if (callbackQueries.Any(x => x.CallbackText.Length > 64 - commandDescriptor.Name.Length - 2))
                throw new TelegramCommandsInternalException("Сallback message is too long");

            _buttons.AddRange(callbackQueries.Select(
                x => new[]
                {
                    CreateNewInlineKeyboardButton(x, commandDescriptor)
                }
            ).ToArray());
            return this;
        }
    }

    public class CallbackData<T>
    {
        public string Text { get; set; }
        public T Data { get; set; }
        public CallbackMode CallbackMode { get; set; }
    }
    
    public class CallbackDataObj
    {
        public string Text { get; set; }
        public object Data { get; set; }
        public CallbackMode CallbackMode { get; set; }
    }

    public enum CallbackMode
    {
        CallbackData = 0,
        InlineCurrent = 1,
        Inline = 2,
        Url = 3,
        Payment = 4
    }

    public class CallbackDataWithCommand<T> : CallbackData<T>
    {
        public ITelegramCommandDescriptor CommandDescriptor { get; set; }
    }
    
    public class CallbackDataWithCommandObj : CallbackDataObj
    {
        public ITelegramCommandDescriptor CommandDescriptor { get; set; }
    }
}