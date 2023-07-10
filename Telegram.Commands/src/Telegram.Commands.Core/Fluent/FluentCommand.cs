using System;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Abstract.Interfaces.Commands;
using Telegram.Commands.Core.Fluent.Builders.StateMachineBuilders;
using Telegram.Commands.Core.Fluent.StateMachine;
using Telegram.Commands.Core.Models;

namespace Telegram.Commands.Core.Fluent
{
    public abstract class FluentCommand<TObject, TStates> : IBehaviorTelegramCommand<FluentObject<TObject, TStates>>
    {
        private StateMachine<TObject,TStates> _stateMachine;
        private StateMachine<TObject, TStates> StateMachineInternal => _stateMachine ??= GetStateMachine();
        public async Task<ITelegramCommandExecutionResult> DefaultExecute<TQuery>(TQuery query,
            FluentObject<TObject, TStates> sessionObject)
        {
            var stateMachine = StateMachineInternal;
            if (sessionObject == null)
            {
                var (obj, nextState) = await Entry(query, default);
                sessionObject = new FluentObject<TObject, TStates>(obj, null)
                {
                    CurrentStateId =
                    {
                        Data = nextState,
                        IsInit = true
                    },
                    ParentStateId = new Initiable<TStates>()
                };
                var entryState = stateMachine.GetStateInternal(sessionObject.CurrentStateId.Data);
                await entryState.SendMessages(query, sessionObject.Object);
                return entryState.NeedAnswer
                    ? TelegramCommandExecutionResult.AheadFluent(this, sessionObject, entryState.DurationInSec)
                    : TelegramCommandExecutionResult.Break();
            }

            if (!sessionObject.CurrentStateId.IsInit)
            {
                var (obj, nextState) = await Entry(query, sessionObject.Object);
                sessionObject.Object = obj;
                sessionObject.CurrentStateId.Data = nextState;
                sessionObject.CurrentStateId.IsInit = true;
                sessionObject.ParentStateId = new Initiable<TStates>();
                if (sessionObject.FireType == FireType.Entry)
                {
                    var entryState = stateMachine.GetStateInternal(sessionObject.CurrentStateId.Data);
                    await entryState.SendMessages(query, sessionObject.Object);
                    return entryState.NeedAnswer ? TelegramCommandExecutionResult.AheadFluent(this, sessionObject, entryState.DurationInSec) : TelegramCommandExecutionResult.Break();
                }
            }

            var currentState = stateMachine.GetStateInternal(sessionObject.CurrentStateId.Data);
            if (sessionObject.ParentStateId.IsInit)
                currentState.SetParentState(sessionObject.ParentStateId.Data);
            var (nextStateId, force) = await currentState.HandleQuery(query, sessionObject.Object);
            
            var next = stateMachine.GetStateInternal(nextStateId);
            if (next.Id.ToString() != currentState.Id.ToString())
            {
                sessionObject.ParentStateId.Data = currentState.Id;
                sessionObject.ParentStateId.IsInit = true;
            }

            if (next.GetStateType() == StateType.Finish)
            {
                return await next.Finalize(query, sessionObject.Object);
            }

            if (next.Id.ToString() != currentState.Id.ToString() || force)
            {
                await next.SendMessages(query, sessionObject.Object);
            }

            sessionObject.CurrentStateId.Data = next.Id;
            return next.NeedAnswer
                ? TelegramCommandExecutionResult.AheadFluent(this, sessionObject, next.DurationInSec)
                : TelegramCommandExecutionResult.Break();
        }

        private StateMachine<TObject, TStates> GetStateMachine()
        {
            var builderStateMachine = new StateMachineBuilder<TObject, TStates>();
            return (StateMachine<TObject, TStates>)StateMachine(builderStateMachine);
        }


        public async Task<ITelegramCommandExecutionResult> Execute<TQuery>(IQueryTelegramCommand<TQuery> currentCommand,
            TQuery query, FluentObject<TObject, TStates> sessionObject)
        {
            var stateMachine = StateMachineInternal;
            if (sessionObject.CurrentStateId == null)
                return await DefaultExecute(query, sessionObject);

            var currentState = stateMachine.GetStateInternal(sessionObject.CurrentStateId.Data);
            if (await currentState.IsCommandHandle(sessionObject.Object, currentCommand))
            {
                return await currentCommand.Execute(query);
            }

            return await DefaultExecute(query, sessionObject);
        }

        public async Task<ITelegramCommandExecutionResult> Execute<TQuery>(
            ISessionTelegramCommand<TQuery, FluentObject<TObject, TStates>> currentCommand, TQuery query,
            FluentObject<TObject, TStates> sessionObject)
        {
            return await DefaultExecute(query, sessionObject);
        }

        protected abstract Task<(TObject, TStates)> Entry<TQuery>(TQuery query, TObject currentObject);

        protected abstract IStateMachine<TStates> StateMachine(IStateMachineBuilder<TObject, TStates> builder);
    }
}