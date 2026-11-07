using System;
using System.Threading.Tasks;
using Bot.Commands.Core.Fluent.Builders.StateBuilders;

namespace Bot.Commands.Core.Fluent.Builders.CallbackBuilders
{
    public interface ICallbacksBuilder<TObj, TStates> : IStateBuilder<TObj, TStates>
    {
        ICallbackRowBuilder<TObj, TStates> Row();
        ICallbacksBuilder<TObj, TStates> KeyBoard(Func<TStates, TObj, ICallbacksBuilderBase<TObj, TStates>, Task> provider);
    }
}