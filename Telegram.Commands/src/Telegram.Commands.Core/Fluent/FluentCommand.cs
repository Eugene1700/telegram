using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Commands;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Exceptions;
using Telegram.Commands.Core.Fluent.Builders.CallbackBuilders;
using Telegram.Commands.Core.Fluent.Builders.StateMachineBuilders;
using Telegram.Commands.Core.Fluent.StateMachine;
using Telegram.Commands.Core.Models;

namespace Telegram.Commands.Core.Fluent
{
    public abstract class FluentCommand<TProps, TStates> : IBehaviorTelegramCommand<FluentObject<TProps, TStates>>
    {
        private Activity<TProps, TStates> _activity;

        private readonly ConcurrentDictionary<string, Func<TStates, TProps, ICallbacksBuilderBase<TProps, TStates>, Task>>
            _buffer = new();
        private Activity<TProps, TStates> ActivityInternal => _activity ??= GetActivity();

        public async Task<ITelegramCommandExecutionResult> DefaultExecute<TQuery>(TQuery query,
            FluentObject<TProps, TStates> sessionObject)
        {
            var stateMachine = ActivityInternal;
            var commandInfo = this.GetCommandInfo();
            if (sessionObject == null)
            {
                if (!commandInfo.DirectCallAllowed)
                {
                    throw new TelegramCommandsDirectCallProhibitedException("Direct call is not allowed");
                }
                var (obj, nextState) = await DidMount(query, default);
                sessionObject = new FluentObject<TProps, TStates>(obj, null)
                {
                    CurrentStateId =
                    {
                        Data = nextState,
                        IsInit = true
                    },
                };

                var entryState = stateMachine.GetStateInternal(sessionObject.CurrentStateId.Data);
                var resGlobalIntercept1 = await UseEffect(query, sessionObject.Object);
                if (resGlobalIntercept1 == UseEffectResult.Freeze)
                    return  TelegramCommandExecutionResult.AheadFluent(this, sessionObject, entryState.DurationInSec);

                if (entryState.GetStateType() == StateType.Finish)
                {
                    return await entryState.Finalize(query, sessionObject.Object);
                }
                
                await entryState.SendMessages(query, sessionObject.Object);
                return entryState.NeedAnswer
                    ? TelegramCommandExecutionResult.AheadFluent(this, sessionObject, entryState.DurationInSec)
                    : TelegramCommandExecutionResult.Break();
            }

            if (!sessionObject.CurrentStateId.IsInit)
            {
                if (!commandInfo.DirectCallAllowed)
                {
                    throw new TelegramCommandsDirectCallProhibitedException("Direct call is not allowed");
                }
                var (obj, nextState) = await DidMount(query, sessionObject.Object);
                sessionObject.Object = obj;
                sessionObject.CurrentStateId.Data = nextState;
                sessionObject.CurrentStateId.IsInit = true;
                var entryState = stateMachine.GetStateInternal(sessionObject.CurrentStateId.Data);
                var resGlobalIntercept2 = await UseEffect(query, sessionObject.Object);
                if (resGlobalIntercept2 == UseEffectResult.Freeze)
                    return TelegramCommandExecutionResult.AheadFluent(this, sessionObject, entryState.DurationInSec);
                if (sessionObject.FireType == FireType.Entry)
                {
                    if (entryState.GetStateType() == StateType.Finish)
                    {
                        return await entryState.Finalize(query, sessionObject.Object);
                    }
                    
                    await entryState.SendMessages(query, sessionObject.Object);
                    return entryState.NeedAnswer
                        ? TelegramCommandExecutionResult.AheadFluent(this, sessionObject, entryState.DurationInSec)
                        : TelegramCommandExecutionResult.Break();
                }
            }

            var currentState = stateMachine.GetStateInternal(sessionObject.CurrentStateId.Data);
            var resGlobalIntercept = await UseEffect(query, sessionObject.Object);
            if (resGlobalIntercept == UseEffectResult.Freeze)
                return TelegramCommandExecutionResult.AheadFluent(this, sessionObject, currentState.DurationInSec);
            
            var (nextStateId, forceRerender) = await currentState.HandleQuery(query, sessionObject.Object);

            var next = stateMachine.GetStateInternal(nextStateId);

            if (next.GetStateType() == StateType.Finish)
            {
                return await next.Finalize(query, sessionObject.Object);
            }

            if (next.Id.ToString() != currentState.Id.ToString() || forceRerender)
            {
                await next.SendMessages(query, sessionObject.Object);
            }

            sessionObject.CurrentStateId.Data = next.Id;
            return next.NeedAnswer
                ? TelegramCommandExecutionResult.AheadFluent(this, sessionObject, next.DurationInSec)
                : TelegramCommandExecutionResult.Break();
        }


        public async Task<ITelegramCommandExecutionResult> Execute<TQuery>(IQueryTelegramCommand<TQuery> currentCommand,
            TQuery query, FluentObject<TProps, TStates> sessionObject)
        {
            var stateMachine = ActivityInternal;
            if (sessionObject.CurrentStateId == null)
                return await DefaultExecute(query, sessionObject);
            var currentState = stateMachine.GetStateInternal(sessionObject.CurrentStateId.Data);
            
            var resGlobalIntercept = await UseEffect(query, sessionObject.Object);
            if (resGlobalIntercept == UseEffectResult.Freeze)
                return TelegramCommandExecutionResult.AheadFluent(this, sessionObject, currentState.DurationInSec);
            
            var interceptResult = UseEffect(sessionObject.CurrentStateId.Data, currentCommand, query,
                sessionObject.Object);
            if (interceptResult.MustEffected || interceptResult.MustEffectedWithoutCallback)
            {
                if (!interceptResult.MustEffectedWithoutCallback)
                {
                    var executionResult = await currentCommand.Execute(query);
                    if (interceptResult.IsTerminal)
                        return executionResult;
                    if (interceptResult.Next != null)
                    {
                        await interceptResult.Next(query, sessionObject.Object, executionResult);
                    }
                }

                return TelegramCommandExecutionResult.AheadFluent(this, sessionObject, currentState.DurationInSec);
            }
            
            if (await currentState.IsCommandHandle(sessionObject.Object, currentCommand))
            {
                return await currentCommand.Execute(query);
            }

            return await DefaultExecute(query, sessionObject);
        }

        protected virtual UseEffectResult<TQuery, TProps> UseEffect<TQuery>(TStates state,
            IQueryTelegramCommand<TQuery> currentCommand, TQuery query,
            TProps sessionObject)
        {
            return new UseEffectResult<TQuery, TProps>
            {
                IsTerminal = false,
                MustEffected = false,
                Next = null,
                MustEffectedWithoutCallback = false
            };
        }

        protected virtual Task<UseEffectResult> UseEffect<TQuery>(TQuery query,
            TProps sessionObject)
        {
            return Task.FromResult(UseEffectResult.Next);
        }

        public async Task<ITelegramCommandExecutionResult> Execute<TQuery>(
            ISessionTelegramCommand<TQuery, FluentObject<TProps, TStates>> currentCommand, TQuery query,
            FluentObject<TProps, TStates> sessionObject)
        {
            return await DefaultExecute(query, sessionObject);
        }

        protected abstract Task<(TProps, TStates)> DidMount<TQuery>(TQuery query, TProps currentObject);

        protected abstract IActivity<TStates> Render(IActivityBuilder<TProps, TStates> builder);

        protected bool ToBuffer(string key, Func<TStates, TProps, ICallbacksBuilderBase<TProps, TStates>, Task> provider)
        {
            return _buffer.TryAdd(key, provider);
        }
        
        protected Func<TStates, TProps, ICallbacksBuilderBase<TProps, TStates>, Task> FromBuffer(string key)
        {
            return _buffer[key];
        }
        
        private Activity<TProps, TStates> GetActivity()
        {
            var builderStateMachine = new ActivityBuilder<TProps, TStates>();
            return (Activity<TProps, TStates>)Render(builderStateMachine);
        }
    }

    public enum UseEffectResult
    {
        Freeze,
        Next
    }

    public class UseEffectResult<TQuery, TObject>
    {
        public bool MustEffected { get; set; }
        public bool MustEffectedWithoutCallback { get; set; }
        public bool IsTerminal { get; set; }
        public Func<TQuery, TObject, ITelegramCommandExecutionResult, Task> Next { get; set; }
    }
}