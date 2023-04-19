using System;
using System.Threading.Tasks;
using Telegram.Commands.Core.Fluent.Builders.StateBuilders;

namespace Telegram.Commands.Core.Fluent.Builders.CallbackBuilders;

public interface ICallbacksBuilder<TObj> : IStateBuilderBase<TObj>
{
    ICallbackRowBuilder<TObj> Row();
    ICallbacksBuilder<TObj> KeyBoard(Func<TObj, ICallbacksBuilderBase<TObj>, Task> provider);
}