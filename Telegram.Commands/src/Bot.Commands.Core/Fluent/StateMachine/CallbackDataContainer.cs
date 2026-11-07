using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Abstract.Messages;
using Bot.Commands.Core.Models;

namespace Bot.Commands.Core.Fluent.StateMachine
{
    internal class CallbackDataContainer<TObj, TStates>
    {
        private readonly Func<TStates, TObj, CallbackDataWithCommand> _builder;
        private readonly ICommandDescriptor _commandDescriptor;
        private readonly Func<IQueryContext<object>, TStates, TObj, string, Task<TStates>> _handler;
        private readonly bool _forceNext;
        private const string _fcudKey = "fud";
        private const string _fcbidKey = "fid";
        public string CallbackKey { get; }

        public CallbackDataContainer(string callbackId, Func<TStates, TObj, CallbackData> provider,
            Func<IQueryContext<object>, TStates, TObj, string, Task<TStates>> handler = null, bool force = false)
        {
            CallbackDataWithCommand NewProvider(TStates s, TObj o)
            {
                var callbackData = provider(s, o);
                return
                    new CallbackDataWithCommand
                    {
                        CallbackMode = callbackData.CallbackMode,
                        CallbackText =
                            $"{_fcbidKey}={callbackId}&{_fcudKey}={Uri.EscapeDataString(callbackData.CallbackText ?? string.Empty)}",
                        Text = callbackData.Text
                    };
            }

            CallbackKey = callbackId;
            _builder = NewProvider;
            _commandDescriptor = null;
            _handler = handler;
            _forceNext = force;
        }

        public CallbackDataContainer(Func<TStates, TObj, CallbackData> provider,
            ICommandDescriptor commandDescriptor)
        {
            CallbackDataWithCommand NewProvider(TStates s, TObj o)
            {
                var callbackData = provider(s, o);
                return new CallbackDataWithCommand
                {
                    CallbackMode = callbackData.CallbackMode, CallbackText = callbackData.CallbackText,
                    Text = callbackData.Text, CommandDescriptor = commandDescriptor
                };
            }

            CallbackKey = default;
            _builder = NewProvider;
            _commandDescriptor = commandDescriptor;
            _handler = null;
            _forceNext = false;
        }

        public CallbackDataWithCommand Build(TStates state, TObj obj)
        {
            if (_builder != null)
            {
                var c = _builder(state, obj);
                return c;
            }

            throw new InvalidOperationException("Callback builder was not set");
        }

        public async Task<(TStates, bool)> Handle<TQuery>(IQueryContext<TQuery> query, TStates state, TObj obj, string callbackUserData)
        {
            return (await _handler?.Invoke((IQueryContext<object>)query, state, obj, callbackUserData), _forceNext);
        }

        public bool HasCommand(ICommandDescriptor descriptor)
        {
            return _commandDescriptor != null && _commandDescriptor.MatchCommand(descriptor);
        }

        public static bool IsCallback<TQuery>(IQueryContext<TQuery> query)
        {
            var data = query.GetData();
            return !string.IsNullOrWhiteSpace(data) && data.Split("&").Any(x => x.StartsWith(_fcbidKey + "=", StringComparison.Ordinal));
        }

        public static (string, string) ExtractData<TQuery>(IQueryContext<TQuery> query)
        {
            var data = query.GetData();
            var parametrs = data.Split("&").Select(x =>
            {
                var parts = x.Split('=', 2);
                if (parts.Length < 2)
                {
                    throw new InvalidOperationException("Callback data format is incorrect");
                }

                return new KeyValuePair<string, string>(parts[0], parts[1]);
            }).ToArray();
            var callbackId = parametrs.SingleOrDefault(x => x.Key == _fcbidKey).Value;
            var userData = parametrs.SingleOrDefault(x => x.Key == _fcudKey).Value;
            return (callbackId, userData == null ? null : Uri.UnescapeDataString(userData));
        }
    }
}
