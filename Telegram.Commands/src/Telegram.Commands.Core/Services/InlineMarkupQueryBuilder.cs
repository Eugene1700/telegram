using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Commands.Abstract.Commands;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Exceptions;

namespace Telegram.Commands.Core.Services
{
    public class InlineMarkupQueryBuilder: IInlineMarkupQueryBuilder
    {
        private readonly List<InlineKeyboardButton[]> _buttons;

        public InlineMarkupQueryBuilder()
        {
            _buttons = new List<InlineKeyboardButton[]>();
        }

        public IInlineMarkupQueryBuilder AddInlineKeyboardButtons<TCommand>(ICallbackData[] callbackQueries)
            where TCommand : IQueryTelegramCommand<CallbackQuery>
        {
            var commandDescriptor = TelegramCommandExtensions.GetCommandInfo<TCommand, CallbackQuery>();
            return AddInlineKeyBoardButtonsInternal(callbackQueries, commandDescriptor);
        }
        
        public IInlineMarkupQueryBuilder AddInlineKeyboardButtons<TCommand, TSessionObject>(ICallbackData[] callbackQueries)
            where TCommand : ISessionTelegramCommand<CallbackQuery, TSessionObject>
        {
            var commandDescriptor = TelegramCommandExtensions.GetCommandInfo<TCommand, CallbackQuery, TSessionObject>();
            return AddInlineKeyBoardButtonsInternal(callbackQueries, commandDescriptor);
        }

        public IInlineMarkupQueryBuilder AddInlineKeyboardButton<TCommand>(ICallbackData callbackData)
            where TCommand : IQueryTelegramCommand<CallbackQuery>
        {
            return AddInlineKeyboardButtons<TCommand>(new[] {callbackData});
        }
        
        public IInlineMarkupQueryBuilder AddInlineKeyboardButton<TCommand, TSessionObject>(ICallbackData callbackData)
            where TCommand : ISessionTelegramCommand<CallbackQuery, TSessionObject>
        {
            return AddInlineKeyboardButtons<TCommand, TSessionObject>(new[] {callbackData});
        }

        public IInlineMarkupQueryBuilder AddInlineKeyboardButton(ICallbackData[] callbackQueries)
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

        public IInlineMarkupQueryBuilder AddInlineKeyboardButton(ICallbackData callbackData)
        {
            return AddInlineKeyboardButton(new[] {callbackData});
        }

        public IInlineMarkupQueryBuilder InlineKeyboardButtonsRow(ICallbackDataWithCommand[] callbackData)
        {
            _buttons.Add(callbackData.Select(CreateNewInlineKeyboardButton).ToArray());
            return this;
        }
        
        public IInlineMarkupQueryBuilder AddInlineKeyboardButton(ICallbackDataWithCommand callbackData)
        {
            _buttons.Add(new[] { CreateNewInlineKeyboardButton(callbackData) });
            return this;
        }

        public IReplyMarkup GetResult()
        {
            return new InlineKeyboardMarkup(_buttons);
        }
        
        public InlineKeyboardMarkup GetInlineKeyboardMarkup()
        {
            return new InlineKeyboardMarkup(_buttons);
        }

        private InlineKeyboardButton CreateNewInlineKeyboardButton(ICallbackDataWithCommand callbackDataWithCommand)
        {
            return CreateNewInlineKeyboardButton(callbackDataWithCommand, callbackDataWithCommand.CommandDescriptor);
        }

        private static InlineKeyboardButton CreateNewInlineKeyboardButton(ICallbackData x,
            ITelegramCommandDescriptor commandDescriptor = null)
        {

            switch (x.CallbackMode)
            {
                case CallbackMode.CallbackData:
                    var callbackData = commandDescriptor == null
                        ? x.CallbackText
                        : $"/{commandDescriptor.Name} {x.CallbackText}";
                    return InlineKeyboardButton.WithCallbackData(x.Text, callbackData);
                case CallbackMode.InlineCurrent:
                    return InlineKeyboardButton.WithSwitchInlineQueryCurrentChat(x.Text, x.CallbackText);
                case CallbackMode.Inline:
                    return InlineKeyboardButton.WithSwitchInlineQuery(x.Text, x.CallbackText);
                case CallbackMode.Url:
                    return InlineKeyboardButton.WithUrl(x.Text, x.CallbackText);
                case CallbackMode.Payment:
                    return InlineKeyboardButton.WithPayment(x.Text);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private InlineMarkupQueryBuilder AddInlineKeyBoardButtonsInternal(
            ICallbackData[] callbackQueries, ITelegramCommandDescriptor commandDescriptor)
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

    public class CallbackData: ICallbackData
    {
        public string Text { get; set; }
        public string CallbackText { get; set; }
        public CallbackMode CallbackMode { get; set; }
    }

    public class CallbackDataWithCommand : CallbackData, ICallbackDataWithCommand
    {
        public ITelegramCommandDescriptor CommandDescriptor { get; set; }
    }
}