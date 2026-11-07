using System;
using System.Linq;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Messages;
using Bot.Commands.Core.Fluent.Builders.CallbackBuilders;
using Bot.Commands.Core.Models;
using Bot.Commands.Core.Services;

namespace Bot.Commands.Core.Fluent.StateMachine
{
    internal class MessageContainer<TObj, TStates>
    {
        private readonly Func<TStates, TObj, Task<ITextMessage>> _message;
        private readonly Func<IQueryContext<object>, TStates, TObj, IBotMessage, Task>  _sendMessageProvider;
        internal CallbackBuilder<TObj, TStates> CallbackBuilder { get; }

        public MessageContainer(string prefix, 
            Func<TStates, TObj, Task<ITextMessage>> messageProvider, 
            Func<IQueryContext<object>, TStates, TObj, IBotMessage, Task>  sendMessageProvider,
            IState<TObj, TStates> currentState) :
            this(messageProvider, sendMessageProvider, new CallbackBuilder<TObj, TStates>($"{prefix}mc", currentState))
        {
        }

        private MessageContainer(Func<TStates, TObj, Task<ITextMessage>> messageProvider, 
            Func<IQueryContext<object>, TStates, TObj, IBotMessage, Task>  sendMessageProvider,
            CallbackBuilder<TObj, TStates> callbackBuilder)
        {
            _message = messageProvider;
            _sendMessageProvider = sendMessageProvider;
            CallbackBuilder = callbackBuilder;
        }

        public async Task<IBotMessage> SendMessage<TQuery>(IQueryContext<TQuery> currentQuery, TStates state, TObj obj, bool useOwnSender)
        {
            var textMessage = await _message(state, obj);
            var callbackBuilder = new InlineMarkupQueryBuilder();
            if (CallbackBuilder != null)
            {
                var callbacks = await CallbackBuilder.Build(state, obj);
                if (callbacks != null && callbacks.Any())
                {
                    foreach (var callbacksContainerRowsProvider in callbacks)
                    {
                        var callbacksRow = callbacksContainerRowsProvider.GetContainers();
                        var row = callbacksRow.Select(x => x.Build(state, obj)).ToArray();
                        callbackBuilder.InlineKeyboardButtonsRow(row);
                    }
                }
            }
            var mes = new BotMessage(textMessage.Text, textMessage.Photo, textMessage.Format,
                callbackBuilder.GetCallbacks()) { SendAsFile = textMessage.SendAsFile, FileName = textMessage.FileName };
            if (useOwnSender)
                await _sendMessageProvider((IQueryContext<object>)currentQuery, state, obj, mes);
            return mes;
        }

        public async Task<(bool, (TStates, bool))> TryHandleCallback<TQuery>(IQueryContext<TQuery> query, TStates state, TObj obj)
        {
            var callbacks = await CallbackBuilder.Build(state, obj);
            var (callbackKey, callbackUserData) =
                CallbackDataContainer<TObj, TStates>.ExtractData(query);
            CallbackDataContainer<TObj, TStates> container = null;
            if (callbacks.Any(x => x.TryGetByKey(callbackKey, obj, out container)))
            {
                return (true, await container.Handle(query, state, obj, callbackUserData));
            }

            return (false, default);
        }

        public async Task<bool> IsCommandHandle<TQuery>(TStates state, TObj obj, IQueryCommand<TQuery> currentCommand)
        {
            var curComDesc = currentCommand.GetCommandInfo();
            var callbacks = await CallbackBuilder.Build(state, obj);
            return callbacks.Any(x => x.HasCommand(curComDesc));
        }
    }
}
