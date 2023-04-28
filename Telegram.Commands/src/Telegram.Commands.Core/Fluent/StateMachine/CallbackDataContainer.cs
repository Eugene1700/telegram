using System;
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
    internal class CallbackDataContainer<TObj, TStates, TCallbacks> where TCallbacks : struct, Enum
    {
        private readonly Func<TObj, (CallbackDataWithCommand, string)> _builder;
        private readonly ITelegramCommandDescriptor _telegramCommandDescriptor;
        private readonly Func<object, TObj, string, Task<TStates>> _handler;
        private readonly bool _forceNext;
        private const string _fcudKey = "fud";
        private const string _fcbidKey = "fid";
        private const string _fcbidHashKey = "fh";
        public TCallbacks CallbackKey { get; }

        public CallbackDataContainer(TCallbacks callbackId, Func<TObj, CallbackData> provider,
            Func<object, TObj, string, Task<TStates>> handler = null, bool force = false)
        {
            (CallbackDataWithCommand, string) NewProvider(TObj o)
            {
                var callbackData = provider(o);
                var hash = ComputeHash(GetHashableString(callbackData));
                return (
                    new CallbackDataWithCommand
                    {
                        CallbackMode = callbackData.CallbackMode,
                        CallbackText =
                            $"{_fcbidKey}={callbackId}&{_fcbidHashKey}={hash}&{_fcudKey}={callbackData.CallbackText}",
                        Text = callbackData.Text
                    }, hash);
            }

            CallbackKey = callbackId;
            _builder = NewProvider;
            _telegramCommandDescriptor = null;
            _handler = handler;
            _forceNext = force;
        }

        public CallbackDataContainer(Func<TObj, CallbackData> provider, 
            ITelegramCommandDescriptor telegramCommandDescriptor)
        {
            (CallbackDataWithCommand, string) NewProvider(TObj o)
            {
                var callbackData = provider(o);
                return (
                    new CallbackDataWithCommand
                    {
                        CallbackMode = callbackData.CallbackMode, CallbackText = callbackData.CallbackText,
                        Text = callbackData.Text, CommandDescriptor = telegramCommandDescriptor
                    }, null);
            }

            CallbackKey = default;
            _builder = NewProvider;
            _telegramCommandDescriptor = telegramCommandDescriptor;
            _handler = null;
            _forceNext = false;
        }

        public CallbackDataWithCommand Build(TObj obj)
        {
            if (_builder != null)
            {
                var (c, _) = _builder(obj);
                return c;
            }

            throw new InvalidOperationException();
        }

        public async Task<(TStates, bool)> Handle<TQuery>(TQuery query, TObj obj, string callbackUserData)
        {
            return (await _handler?.Invoke(query, obj, callbackUserData), _forceNext);
        }

        public bool HasCommand(ITelegramCommandDescriptor descriptor)
        {
            return _telegramCommandDescriptor != null && _telegramCommandDescriptor.MatchCommand(descriptor);
        }

        public static bool IsCallback<TQuery>(TQuery query)
        {
            var data = query.GetData();
            return data.Split("&").Any(x => x.StartsWith(_fcbidKey));
        }

        public static (TCallbacks,string, string) ExtractData<TQuery>(TQuery query)
        {
            var data = query.GetData();
            var parametrs = data.Split("&").Select(x =>
            {
                var parts = x.Split("=");
                if (parts.Length < 2)
                {
                    throw new InvalidOperationException();
                }
                return new KeyValuePair<string, string>(parts[0], parts[1]);
            }).ToArray();
            var callbackId = parametrs.SingleOrDefault(x => x.Key == _fcbidKey).Value;
            var hash = parametrs.SingleOrDefault(x => x.Key == _fcbidHashKey).Value;
            var userData = parametrs.SingleOrDefault(x => x.Key == _fcudKey).Value;
            return (Enum.Parse<TCallbacks>(callbackId), hash, userData);
        }

        public string GetHash(TObj obj)
        {
            var (_,h) = _builder(obj);
            return h;
        }
    
        private static string ByteArrayToString(byte[] arrInput)
        {
            int i;
            StringBuilder sOutput = new StringBuilder(arrInput.Length);
            for (i=0;i < arrInput.Length -1; i++)
            {
                sOutput.Append(arrInput[i].ToString("X2"));
            }
            return sOutput.ToString();
        }
    
        private string GetHashableString(CallbackData callbackData)
        {
            return $"{callbackData.Text}{callbackData.CallbackText}{callbackData.CallbackMode}";
        }
    
        private static string ComputeHash(string source)
        {
            var tmpSource = Encoding.Unicode.GetBytes(source);
            var tmpNewHash = MD5.Create().ComputeHash(tmpSource);
            var byteArrayToString = ByteArrayToString(tmpNewHash);
            return byteArrayToString.Substring(byteArrayToString.Length - 8);
        }
    }
}