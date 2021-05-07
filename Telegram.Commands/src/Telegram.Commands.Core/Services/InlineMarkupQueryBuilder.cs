using System.Collections.Generic;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using System.Linq;
using Telegram.Commands.Abstract;
using Telegram.Commands.Abstract.Interfaces;
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

        public InlineMarkupQueryBuilder AddInlineKeyboardButtons<TCommand>(CallbackData[] callbackQueries) where TCommand : ITelegramCommand<CallbackQuery>
        {
            var commandDescriptor = TelegramCommandExtensions.GetCommandInfo<TCommand, CallbackQuery>();
            if (callbackQueries.Any(x=>x.CallbackText.Length > 64 - commandDescriptor.Name.Length - 2))
                throw new TelegramCommandsInternalException("Сallback message is too long");

            _buttons.AddRange(callbackQueries.Select(
                x => new[]
                {
                    CreateNewInlineKeyboardButton(x, commandDescriptor)
                }
            ).ToArray());
            return this;
        }

        private static InlineKeyboardButton CreateNewInlineKeyboardButton(CallbackData x, 
            ITelegramCommandDescriptor commandDescriptor)
        {
            return new InlineKeyboardButton
            {
                Text = x.Text,
                CallbackData = $"/{commandDescriptor.Name} {x.CallbackText}",
            };
        }

        public InlineMarkupQueryBuilder AddInlineKeyboardButton<TCommand>(CallbackData callbackData) where TCommand : ITelegramCommand<CallbackQuery>
        {
            return AddInlineKeyboardButtons<TCommand>(new[] {callbackData});
        }
        
        public InlineMarkupQueryBuilder AddInlineKeyboardButton(CallbackData[] callbackQueries)
        {
            if (callbackQueries.Any(x=>x.CallbackText.Length > 64))
                throw new TelegramCommandsInternalException("Сallback message is too long");

            _buttons.AddRange(callbackQueries.Select(
                x => new[]
                {
                    new InlineKeyboardButton
                    {
                        Text = x.Text,
                        CallbackData = $"{x.CallbackText}",
                    }
                }
            ).ToArray());
            return this;
        }

        public InlineMarkupQueryBuilder AddInlineKeyboardButton(CallbackData callbackData)
        {
            return AddInlineKeyboardButton(new[] {callbackData});
        }

        public InlineMarkupQueryBuilder InlineKeyboardButtonsRow(CallbackDataWithCommand[] callbackData)
        {
            _buttons.Add(callbackData.Select(CreateNewInlineKeyboardButton).ToArray());
            return this;
        }

        private InlineKeyboardButton CreateNewInlineKeyboardButton(CallbackDataWithCommand callbackDataWithCommand)
        {
            return CreateNewInlineKeyboardButton(callbackDataWithCommand, callbackDataWithCommand.CommandDescriptor);
        }

        public IReplyMarkup GetResult()
        {
            return new InlineKeyboardMarkup(_buttons);
        }
    }
    
    public class CallbackData
    {
        public string Text { get; set; }
        public string CallbackText { get; set; }
    }

    public class CallbackDataWithCommand : CallbackData
    {
        public ITelegramCommandDescriptor CommandDescriptor { get; set; }
    }
}