using System.Collections.Generic;
using System.Collections.ObjectModel;
using Telegram.Commands.Abstract.Commands;
using Telegram.Commands.Abstract.Messages;

namespace Telegram.Commands.Core.Services
{
    public class InlineMarkupQueryBuilder : IInlineMarkupQueryBuilder
    {
        private readonly List<CallbackDataWithCommand> _callbacks = new();

        public IInlineMarkupQueryBuilder InlineKeyboardButtonsRow(CallbackDataWithCommand[] row)
        {
            _callbacks.AddRange(row);
            return this;
        }

        public ReadOnlyCollection<CallbackDataWithCommand> GetCallbacks()
        {
            return new ReadOnlyCollection<CallbackDataWithCommand>(_callbacks);
        }

        public IInlineMarkupQueryBuilder
            AddInlineKeyboardButton<TCommand, TSessionObject, TQuery>(CallbackData callbackData)
            where TCommand : ISessionTelegramCommand<TQuery, TSessionObject>
        {
            var commandDescriptor = BotCommandExtensions.GetCommandInfo(typeof(TCommand));
            _callbacks.Add(new CallbackDataWithCommand
            {
                Text = callbackData.Text,
                CallbackMode = callbackData.CallbackMode,
                CallbackText = callbackData.CallbackText,
                CommandDescriptor = commandDescriptor
            });
            return this;
        }
    }
}