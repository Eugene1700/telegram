using System.Collections.Generic;
using System.Collections.ObjectModel;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Messages;

namespace Bot.Commands.Core.Services
{
    public class InlineMarkupQueryBuilder : IInlineMarkupQueryBuilder
    {
        private readonly List<CallbackDataWithCommand> _callbacks = new();
        private int _nextRow;

        public IInlineMarkupQueryBuilder InlineKeyboardButtonsRow(CallbackDataWithCommand[] row)
        {
            foreach (var callback in row)
                _callbacks.Add(callback with { Row = _nextRow });
            _nextRow++;
            return this;
        }

        public ReadOnlyCollection<CallbackDataWithCommand> GetCallbacks()
        {
            return new ReadOnlyCollection<CallbackDataWithCommand>(_callbacks);
        }

        public IInlineMarkupQueryBuilder
            AddInlineKeyboardButton<TCommand, TQuery>(CallbackData callbackData)
            where TCommand : IQueryCommand<TQuery>
        {
            return InlineKeyboardButtonsRow(new[] { new CallbackDataWithCommand
            {
                Text = callbackData.Text, CallbackMode = callbackData.CallbackMode,
                CallbackText = callbackData.CallbackText,
                CommandDescriptor = BotCommandExtensions.GetCommandInfo(typeof(TCommand))
            }});
        }

        public IInlineMarkupQueryBuilder
            AddInlineKeyboardButton<TCommand, TSessionObject, TQuery>(CallbackData callbackData)
            where TCommand : ISessionCommand<TQuery, TSessionObject>
        {
            var commandDescriptor = BotCommandExtensions.GetCommandInfo(typeof(TCommand));
            _callbacks.Add(new CallbackDataWithCommand
            {
                Text = callbackData.Text,
                CallbackMode = callbackData.CallbackMode,
                CallbackText = callbackData.CallbackText,
                CommandDescriptor = commandDescriptor,
                Row = _nextRow++
            });
            return this;
        }
    }
}
