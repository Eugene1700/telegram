using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Abstract.Interfaces.Commands;
using Telegram.Commands.Core.Fluent.Builders.CallbackBuilders;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Fluent.StateMachine
{
    internal class State<TObj, TStates, TCallbacks> : IState<TObj, TStates> where TCallbacks : struct, Enum
    {
        private Func<object, TObj, Task<TStates>> _handler;
        private bool _forceNext = false;
    
        private readonly Func<object,TObj,Task<ITelegramCommandExecutionResult>> _finalizer;
        private readonly StateType _stateType;
        public TStates Id { get; }
        public uint? DurationInSec { get; }
        private readonly List<MessageContainer<TObj, TStates, TCallbacks>> _messageContainers;
        private MessageContainer<TObj,TStates,TCallbacks> _currentMessageContainer;

        public State(TStates id, StateType stateType, uint? durationInSec, Func<object, TObj, Task<ITelegramCommandExecutionResult>> finalizer = null)
        {
            _stateType = stateType;
            Id = id;
            DurationInSec = durationInSec;
            _finalizer = finalizer;
            _messageContainers = new List<MessageContainer<TObj, TStates, TCallbacks>>();
        }

        public async Task SendMessages<TQuery>(TQuery currentQuery, TObj obj)
        {
            foreach (var messageContainer in _messageContainers)
            {
                await messageContainer.SendMessage(currentQuery, obj);
            }
        }

        public async Task<(TStates, bool)> HandleQuery<TQuery>(TQuery query, TObj obj)
        {
            if (CallbackDataContainer<TObj, TStates, TCallbacks>.IsCallback(query))
            {
                foreach (var messageContainer in _messageContainers)
                {
                    var (res, resHandle) = await messageContainer.TryHandleCallback(query, obj);
                    if (res)
                    {
                        return resHandle;
                    }
                }

                throw new InvalidOperationException();
            }

            return (await _handler(query, obj), _forceNext);
        }

        public async Task<bool> IsCommandHandle<TQuery>(TObj obj, IQueryTelegramCommand<TQuery> currentCommand)
        {
            foreach (var messageContainer in _messageContainers)
            {
                if (await messageContainer.IsCommandHandle(obj, currentCommand))
                {
                    return true;
                }
            }

            return false;
        }

        public void SetHandler(Func<object, TObj, Task<TStates>> handler, bool force)
        {
            _handler = handler;
            _forceNext = force;
        }

        public StateType GetStateType()
        {
            return _stateType;
        }

        public Task<ITelegramCommandExecutionResult> Finalize<TQuery>(TQuery query, TObj sessionObjectObject)
        {
            return _finalizer?.Invoke(query, sessionObjectObject);
        }

        public void AddMessage(Func<TObj,Task<string>> messageProvider, IMessageSender<TObj> sender)
        {
            var newMessageContainer = new MessageContainer<TObj, TStates, TCallbacks>(messageProvider, sender);
            _messageContainers.Add(newMessageContainer);
            _currentMessageContainer = newMessageContainer;
        }

        public CallbackBuilder<TObj, TStates, TCallbacks> GetCurrentCallbackBuilder()
        {
            return _currentMessageContainer?.CallbackBuilder;
        }
    }

    internal class MessageContainer<TObj, TStates, TCallbacks> where TCallbacks : struct, Enum
    {
        private readonly Func<TObj, Task<string>> _message;
        private readonly IMessageSender<TObj> _sendMessageProvider;
        internal CallbackBuilder<TObj, TStates, TCallbacks> CallbackBuilder { get; }

        public MessageContainer(Func<TObj, Task<string>> messageProvider, IMessageSender<TObj> sendMessageProvider)
        {
            _message = messageProvider;
            _sendMessageProvider = sendMessageProvider;
            CallbackBuilder = new CallbackBuilder<TObj, TStates, TCallbacks>();
        }
    
        public async Task SendMessage<TQuery>(TQuery currentQuery, TObj obj)
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
            await _sendMessageProvider.Send(currentQuery, obj, mes);
        }
    
        public async Task<(bool, (TStates, bool))> TryHandleCallback<TQuery>(TQuery query, TObj obj)
        {
            var callbacks = await CallbackBuilder.Build(obj);
            var (callbackKey, hash, callbackUserData) = CallbackDataContainer<TObj, TStates, TCallbacks>.ExtractData(query);
            CallbackDataContainer<TObj, TStates, TCallbacks> container = null;
            if (callbacks.Any(x => x.TryGetByKey(callbackKey, hash, obj, out container)))
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