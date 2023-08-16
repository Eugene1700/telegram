using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Commands.Abstract.Interfaces.Commands;
using Telegram.Commands.Core.Fluent.Builders.CallbackBuilders;
using Telegram.Commands.Core.Fluent.Builders.StateBuilders;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Fluent.StateMachine
{

    internal class MessageContainer<TObj, TStates>
    {
        private readonly Func<TStates, TObj, Task<IMessageText>> _message;
        private readonly Func<object, TStates, TObj, ITelegramMessage, Task>  _sendMessageProvider;
        internal CallbackBuilder<TObj, TStates> CallbackBuilder { get; }

        public MessageContainer(string prefix, 
            Func<TStates, TObj, Task<IMessageText>> messageProvider, 
            Func<object, TStates, TObj, ITelegramMessage, Task>  sendMessageProvider,
            IState<TObj, TStates> currentState) :
            this(messageProvider, sendMessageProvider, new CallbackBuilder<TObj, TStates>($"{prefix}mc", currentState))
        {
        }

        private MessageContainer(Func<TStates, TObj, Task<IMessageText>> messageProvider, 
            Func<object, TStates, TObj, ITelegramMessage, Task>  sendMessageProvider,
            CallbackBuilder<TObj, TStates> callbackBuilder)
        {
            _message = messageProvider;
            _sendMessageProvider = sendMessageProvider;
            CallbackBuilder = callbackBuilder;
        }

        public async Task<TelegramMessage> SendMessage<TQuery>(TQuery currentQuery, TStates state, TObj obj, bool useOwnSender)
        {
            IReplyMarkup replyMarkup = null;
            var callbacks = await CallbackBuilder?.Build(state, obj);
            if (callbacks != null && callbacks.Any())
            {
                var builder = new InlineMarkupQueryBuilder();
                foreach (var callbacksContainerRowsProvider in callbacks)
                {
                    var callbacksRow = callbacksContainerRowsProvider.GetContainers();
                    var row = callbacksRow.Select(x => x.Build(state, obj)).ToArray();
                    builder.InlineKeyboardButtonsRow(row);
                }

                replyMarkup = builder.GetResult();
            }

            var messageText = await _message(state, obj);
            var mes = new TelegramMessage(messageText, replyMarkup);
            if (useOwnSender)
                await _sendMessageProvider(currentQuery, state, obj, mes);
            return mes;
        }

        public async Task<(bool, (TStates, bool))> TryHandleCallback<TQuery>(TQuery query, TStates state, TObj obj)
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

        public async Task<bool> IsCommandHandle<TQuery>(TStates state, TObj obj, IQueryTelegramCommand<TQuery> currentCommand)
        {
            var curComDesc = currentCommand.GetCommandInfo();
            var callbacks = await CallbackBuilder.Build(state, obj);
            return callbacks.Any(x => x.HasCommand(curComDesc));
        }
    }
}