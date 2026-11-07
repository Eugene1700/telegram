using Bot.Commands.Abstract.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Abstract.Messages;

namespace Bot.Commands.Core.Fluent.StateMachine
{
    internal class CallbackDataContainerRow<TObj, TStates>
    {
        private readonly string _prefix;
        private readonly List<CallbackDataContainer<TObj, TStates>> _containers;

        public CallbackDataContainerRow(string prefix)
        {
            _prefix = prefix;
            _containers = new List<CallbackDataContainer<TObj, TStates>>();
        }

        public bool HasCommand(ICommandDescriptor curComDesc)
        {
            return _containers.Any(x => x.HasCommand(curComDesc));
        }

        public CallbackDataContainer<TObj, TStates> AddContainer<TQuery>(Func<TStates, TObj, CallbackData> callbackProvider, 
            Func<IQueryContext<TQuery>, TStates, TObj, string, Task<TStates>> handler,
            bool force)
            where TQuery : class
        {
            Task<TStates> Handle(IQueryContext<object> q, TStates s, TObj o, string d) => handler(((IQueryContext<TQuery>)q), s, o, d);

            var callbackId = $"{_prefix}c{_containers.Count}";
            var newContainer = new CallbackDataContainer<TObj, TStates>(callbackId, callbackProvider, Handle, force);
            _containers.Add(newContainer);
            return newContainer;
        }

        public ReadOnlyCollection<CallbackDataContainer<TObj, TStates>> GetContainers()
        {
            return _containers.AsReadOnly();
        }

        public CallbackDataContainer<TObj, TStates> AddContainer(Func<TStates, TObj, CallbackData> callbackProvider,
            ICommandDescriptor commandDescriptor)
        {
            var newContainer =
                new CallbackDataContainer<TObj, TStates>(callbackProvider, commandDescriptor);
            _containers.Add(newContainer);
            return newContainer;
        }

        public bool TryGetByKey(string callbackKey, TObj obj,
            out CallbackDataContainer<TObj, TStates> o)
        {
            o = null;
            var container = _containers.FirstOrDefault(x => x.CallbackKey == callbackKey);
            if (container == null) return false;
            o = container;
            return o != null;
        }
    }
}