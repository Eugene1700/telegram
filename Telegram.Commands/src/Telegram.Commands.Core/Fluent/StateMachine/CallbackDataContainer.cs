﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Fluent.StateMachine
{
    internal class CallbackDataContainer<TObj, TStates>
    {
        private readonly Func<TStates, TObj, CallbackDataWithCommand> _builder;
        private readonly ITelegramCommandDescriptor _telegramCommandDescriptor;
        private readonly Func<object, TStates, TObj, string, Task<TStates>> _handler;
        private readonly bool _forceNext;
        private const string _fcudKey = "fud";
        private const string _fcbidKey = "fid";
        public string CallbackKey { get; }

        public CallbackDataContainer(string callbackId, Func<TStates, TObj, CallbackData> provider,
            Func<object, TStates, TObj, string, Task<TStates>> handler = null, bool force = false)
        {
            CallbackDataWithCommand NewProvider(TStates s, TObj o)
            {
                var callbackData = provider(s, o);
                return
                    new CallbackDataWithCommand
                    {
                        CallbackMode = callbackData.CallbackMode,
                        CallbackText =
                            $"{_fcbidKey}={callbackId}&{_fcudKey}={callbackData.CallbackText}",
                        Text = callbackData.Text
                    };
            }

            CallbackKey = callbackId;
            _builder = NewProvider;
            _telegramCommandDescriptor = null;
            _handler = handler;
            _forceNext = force;
        }

        public CallbackDataContainer(Func<TStates, TObj, CallbackData> provider,
            ITelegramCommandDescriptor telegramCommandDescriptor)
        {
            CallbackDataWithCommand NewProvider(TStates s, TObj o)
            {
                var callbackData = provider(s, o);
                return new CallbackDataWithCommand
                {
                    CallbackMode = callbackData.CallbackMode, CallbackText = callbackData.CallbackText,
                    Text = callbackData.Text, CommandDescriptor = telegramCommandDescriptor
                };
            }

            CallbackKey = default;
            _builder = NewProvider;
            _telegramCommandDescriptor = telegramCommandDescriptor;
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

        public async Task<(TStates, bool)> Handle<TQuery>(TQuery query, TStates state, TObj obj, string callbackUserData)
        {
            return (await _handler?.Invoke(query, state, obj, callbackUserData), _forceNext);
        }

        public bool HasCommand(ITelegramCommandDescriptor descriptor)
        {
            return _telegramCommandDescriptor != null && _telegramCommandDescriptor.MatchCommand(descriptor);
        }

        public static bool IsCallback<TQuery>(TQuery query)
        {
            var data = query.GetData();
            return !string.IsNullOrWhiteSpace(data) && data.Split("&").Any(x => x.StartsWith(_fcbidKey));
        }

        public static (string, string) ExtractData<TQuery>(TQuery query)
        {
            var data = query.GetData();
            var parametrs = data.Split("&").Select(x =>
            {
                var parts = x.Split("=");
                if (parts.Length < 2)
                {
                    throw new InvalidOperationException("Callback data format is incorrect");
                }

                return new KeyValuePair<string, string>(parts[0], parts[1]);
            }).ToArray();
            var callbackId = parametrs.SingleOrDefault(x => x.Key == _fcbidKey).Value;
            var userData = parametrs.SingleOrDefault(x => x.Key == _fcudKey).Value;
            return (callbackId, userData);
        }
    }
}