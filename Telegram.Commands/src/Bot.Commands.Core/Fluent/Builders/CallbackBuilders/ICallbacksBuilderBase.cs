using System;
using System.Threading.Tasks;

namespace Bot.Commands.Core.Fluent.Builders.CallbackBuilders
{
    public interface ICallbacksBuilderBase<TObj, TStates>
    {
        ICallbackRowBuilderBase<TObj, TStates> Row();
        ICallbacksBuilderBase<TObj, TStates> Keyboard(Func<TStates, TObj, ICallbacksBuilderBase<TObj, TStates>, Task> provider);
    }
}