using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Commands.Abstract.Interfaces.Commands;
using Telegram.Commands.Core.Fluent.Builders.CallbackBuilders;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Fluent.StateMachine
{

    internal class MessageContainer<TObj, TStates>
    {
        private readonly Func<TObj, Task<string>> _message;
        private readonly Func<object, TObj, ITelegramMessage, Task>  _sendMessageProvider;
        internal CallbackBuilder<TObj, TStates> CallbackBuilder { get; }

        public MessageContainer(string prefix, 
            Func<TObj, Task<string>> messageProvider, 
            Func<object, TObj, ITelegramMessage, Task>  sendMessageProvider) :
            this(messageProvider, sendMessageProvider, new CallbackBuilder<TObj, TStates>($"{prefix}mc"))
        {
        }

        internal MessageContainer(Func<TObj, Task<string>> messageProvider, 
            Func<object, TObj, ITelegramMessage, Task>  sendMessageProvider,
            CallbackBuilder<TObj, TStates> callbackBuilder)
        {
            _message = messageProvider;
            _sendMessageProvider = sendMessageProvider;
            CallbackBuilder = callbackBuilder;
        }

        public async Task<TelegramMessage> SendMessage<TQuery>(TQuery currentQuery, TObj obj, bool useOwnSender)
        {
            IReplyMarkup replyMarkup = null;
            var callbacks = await CallbackBuilder?.Build(obj);
            if (callbacks != null && callbacks.Any())
            {
                var builder = new InlineMarkupQueryBuilder();
                foreach (var callbacksContainerRowsProvider in callbacks)
                {
                    var callbacksRow = callbacksContainerRowsProvider.GetContainers();
                    var row = callbacksRow.Select(x => x.Build(obj)).ToArray();
                    builder.InlineKeyboardButtonsRow(row);
                }

                replyMarkup = builder.GetResult();
            }

            var messageText = await _message(obj);
            var mes = new TelegramMessage(messageText, replyMarkup);
            if (useOwnSender)
                await _sendMessageProvider(currentQuery, obj, mes);
            return mes;
        }

        public async Task<(bool, (TStates, bool))> TryHandleCallback<TQuery>(TQuery query, TObj obj)
        {
            var callbacks = await CallbackBuilder.Build(obj);
            var (callbackKey, callbackUserData) =
                CallbackDataContainer<TObj, TStates>.ExtractData(query);
            CallbackDataContainer<TObj, TStates> container = null;
            if (callbacks.Any(x => x.TryGetByKey(callbackKey, obj, out container)))
            {
                return (true, await container.Handle(query, obj, callbackUserData));
            }

            return (false, default);
        }

        public async Task<bool> IsCommandHandle<TQuery>(TObj obj, IQueryTelegramCommand<TQuery> currentCommand)
        {
            var curComDesc = currentCommand.GetCommandInfo();
            var callbacks = await CallbackBuilder.Build(obj);
            return callbacks.Any(x => x.HasCommand(curComDesc));
        }
    }
}