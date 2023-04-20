using System;

namespace Telegram.Commands.Core.Fluent.StateMachine;

public interface IStateBase<out TStates>
{
    TStates Id { get; }
    uint? DurationInSec { get; }
}