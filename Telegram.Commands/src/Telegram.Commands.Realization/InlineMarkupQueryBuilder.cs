using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Commands.Abstract.Commands;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Abstract.Messages;
using Telegram.Commands.Core;
using Telegram.Commands.Core.Exceptions;

namespace Telegram.Commands.Realization
{
    public class TelegramInlineMarkupQueryBuilder
    {
        private readonly List<InlineKeyboardButton[]> _buttons;

        public TelegramInlineMarkupQueryBuilder()
        {
            _buttons = new List<InlineKeyboardButton[]>();
        }

        public TelegramInlineMarkupQueryBuilder AddInlineKeyboardButtons<TCommand>(CallbackData[] callbackQueries)
            where TCommand : IQueryTelegramCommand<CallbackQuery>
        {
            var commandDescriptor = BotCommandExtensions.GetCommandInfo<TCommand, CallbackQuery>();
            return AddInlineKeyBoardButtonsInternal(callbackQueries, commandDescriptor);
        }
        
        public TelegramInlineMarkupQueryBuilder AddInlineKeyboardButtons<TCommand, TSessionObject>(CallbackData[] callbackQueries)
            where TCommand : ISessionTelegramCommand<CallbackQuery, TSessionObject>
        {
            var commandDescriptor = BotCommandExtensions.GetCommandInfo<TCommand, CallbackQuery, TSessionObject>();
            return AddInlineKeyBoardButtonsInternal(callbackQueries, commandDescriptor);
        }

        public TelegramInlineMarkupQueryBuilder AddInlineKeyboardButton<TCommand>(CallbackData callbackData)
            where TCommand : IQueryTelegramCommand<CallbackQuery>
        {
            return AddInlineKeyboardButtons<TCommand>(new[] {callbackData});
        }
        
        public TelegramInlineMarkupQueryBuilder AddInlineKeyboardButton<TCommand, TSessionObject>(CallbackData callbackData)
            where TCommand : ISessionTelegramCommand<CallbackQuery, TSessionObject>
        {
            return AddInlineKeyboardButtons<TCommand, TSessionObject>(new[] {callbackData});
        }

        public TelegramInlineMarkupQueryBuilder AddInlineKeyboardButton(CallbackData[] callbackQueries)
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

        public TelegramInlineMarkupQueryBuilder AddInlineKeyboardButton(CallbackData callbackData)
        {
            return AddInlineKeyboardButton(new[] {callbackData});
        }
        
        public TelegramInlineMarkupQueryBuilder AddInlineKeyboardButton(CallbackDataWithCommand callbackData)
        {
            _buttons.Add(new[] { CreateNewInlineKeyboardButton(callbackData) });
            return this;
        }

        public TelegramInlineMarkupQueryBuilder InlineKeyboardButtonsRow(CallbackDataWithCommand[] callbackData)
        {
            _buttons.Add(callbackData.Select(CreateNewInlineKeyboardButton).ToArray());
            return this;
        }

        public InlineKeyboardMarkup GetResult()
        {
            return new InlineKeyboardMarkup(_buttons);
        }

        private InlineKeyboardButton CreateNewInlineKeyboardButton(CallbackDataWithCommand callbackDataWithCommand)
        {
            return CreateNewInlineKeyboardButton(callbackDataWithCommand, callbackDataWithCommand.CommandDescriptor);
        }

        private static InlineKeyboardButton CreateNewInlineKeyboardButton(CallbackData x,
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
                    return InlineKeyboardButton.WithPay(x.Text);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        } 
        
        private TelegramInlineMarkupQueryBuilder AddInlineKeyBoardButtonsInternal(
            CallbackData[] callbackQueries, ITelegramCommandDescriptor commandDescriptor)
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
}