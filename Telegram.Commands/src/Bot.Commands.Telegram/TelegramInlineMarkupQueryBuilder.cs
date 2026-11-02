using global::Telegram.Bot.Types;
using global::Telegram.Bot.Types.ReplyMarkups;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Abstract.Messages;
using Bot.Commands.Core;
using Bot.Commands.Core.Exceptions;

namespace Bot.Commands.Telegram
{
    public class TelegramInlineMarkupQueryBuilder
    {
        private readonly List<InlineKeyboardButton[]> _buttons;

        public TelegramInlineMarkupQueryBuilder()
        {
            _buttons = new List<InlineKeyboardButton[]>();
        }

        public TelegramInlineMarkupQueryBuilder AddInlineKeyboardButtons<TCommand>(CallbackData[] callbackQueries)
            where TCommand : IQueryCommand<CallbackQuery>
        {
            var commandDescriptor = BotCommandExtensions.GetCommandInfo<TCommand, CallbackQuery>();
            return AddInlineKeyBoardButtonsInternal(callbackQueries, commandDescriptor);
        }
        
        public TelegramInlineMarkupQueryBuilder AddInlineKeyboardButtons<TCommand, TSessionObject>(CallbackData[] callbackQueries)
            where TCommand : ISessionCommand<CallbackQuery, TSessionObject>
        {
            var commandDescriptor = BotCommandExtensions.GetCommandInfo<TCommand, CallbackQuery, TSessionObject>();
            return AddInlineKeyBoardButtonsInternal(callbackQueries, commandDescriptor);
        }

        public TelegramInlineMarkupQueryBuilder AddInlineKeyboardButton<TCommand>(CallbackData callbackData)
            where TCommand : IQueryCommand<CallbackQuery>
        {
            return AddInlineKeyboardButtons<TCommand>(new[] {callbackData});
        }
        
        public TelegramInlineMarkupQueryBuilder AddInlineKeyboardButton<TCommand, TSessionObject>(CallbackData callbackData)
            where TCommand : ISessionCommand<CallbackQuery, TSessionObject>
        {
            return AddInlineKeyboardButtons<TCommand, TSessionObject>(new[] {callbackData});
        }

        public TelegramInlineMarkupQueryBuilder AddInlineKeyboardButton(CallbackData[] callbackQueries)
        {
            if (callbackQueries.Any(x => x.CallbackText.Length > 64 && x.CallbackMode == CallbackMode.CallbackData))
                throw new CommandsInternalException("Сallback message is too long");
            
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
            ICommandDescriptor commandDescriptor = null)
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
            CallbackData[] callbackQueries, ICommandDescriptor commandDescriptor)
        {
            if (callbackQueries.Any(x => x.CallbackText.Length > 64 - commandDescriptor.Name.Length - 2))
                throw new CommandsInternalException("Сallback message is too long");

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