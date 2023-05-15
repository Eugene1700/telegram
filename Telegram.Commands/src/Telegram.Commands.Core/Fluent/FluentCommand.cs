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
        public async Task<ITelegramCommandExecutionResult> DefaultExecute<TQuery>(TQuery query,
            FluentObject<TObject, TStates> sessionObject)
        {
            var stateMachine = GetStateMachine();
            if (sessionObject == null)
            {
                sessionObject = new FluentObject<TObject, TStates>(await Entry(query, default), null)
                {
                    CurrentStateId =
                    {
                        Data = stateMachine.GetEntryState().Id,
                        IsInit = true
                    },
                };
                var entryState = stateMachine.GetStateInternal(sessionObject.CurrentStateId.Data);
                await entryState.SendMessages(query, sessionObject.Object);
                return TelegramCommandExecutionResult.AheadFluent(this, sessionObject, entryState.DurationInSec);
            }

            if (!sessionObject.CurrentStateId.IsInit)
            {
                var d = await Entry(query, sessionObject.Object);
                sessionObject.Object = d;
                sessionObject.CurrentStateId.Data = stateMachine.GetEntryState().Id;
                sessionObject.CurrentStateId.IsInit = true;
                if (sessionObject.FireType == FireType.Entry)
                {
                    var entryState = stateMachine.GetStateInternal(sessionObject.CurrentStateId.Data);
                    await entryState.SendMessages(query, sessionObject.Object);
                    return TelegramCommandExecutionResult.AheadFluent(this, sessionObject, entryState.DurationInSec);
                }
            }
        
            var currentState = stateMachine.GetStateInternal(sessionObject.CurrentStateId.Data);
            var (nextStateId, force) = await currentState.HandleQuery(query, sessionObject.Object);

            var next = stateMachine.GetStateInternal(nextStateId);
            if (next.GetStateType() == StateType.Finish)
            {
                return await next.Finalize(query, sessionObject.Object);
            }

            if (next.Id.ToString() != currentState.Id.ToString() || force)
            {
                await next.SendMessages(query, sessionObject.Object);
            }

            sessionObject.CurrentStateId.Data = next.Id;
            return TelegramCommandExecutionResult.AheadFluent(this, sessionObject, next.DurationInSec);
        }

        private StateMachine<TObject, TStates> GetStateMachine()
        {
            var builderStateMachine = new StateMachineBuilder<TObject, TStates>();
            var stateMachine = (StateMachine<TObject, TStates>)StateMachine(builderStateMachine);
            return stateMachine;
        }

    
        public async Task<ITelegramCommandExecutionResult> Execute<TQuery>(IQueryTelegramCommand<TQuery> currentCommand,
            TQuery query, FluentObject<TObject, TStates> sessionObject)
        {
            var stateMachine = GetStateMachine();
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

        protected abstract Task<TObject> Entry<TQuery>(TQuery query, TObject currentObject);

        protected abstract IStateMachine<TStates> StateMachine(IStateMachineBuilder<TObject, TStates> builder);

    }
}