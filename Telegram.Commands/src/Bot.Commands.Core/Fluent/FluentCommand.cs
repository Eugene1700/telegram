using System;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Core.Fluent.Builders.StateMachineBuilders;
using Bot.Commands.Core.Fluent.StateMachine;
using Bot.Commands.Core.Models;

namespace Bot.Commands.Core.Fluent
{
    public abstract class FluentCommand<TObject, TStates> : IBehaviorCommand<FluentObject<TObject, TStates>>
    {
        private StateMachine<TObject, TStates> _stateMachine;
        private StateMachine<TObject, TStates> StateMachineInternal => _stateMachine ??= GetStateMachine();

        public async Task<ICommandExecutionResult> DefaultExecute<TQuery>(IQueryContext<TQuery> query,
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
                };

                var entryState = stateMachine.GetStateInternal(sessionObject.CurrentStateId.Data);
                var resGlobalIntercept1 = await GlobalIntercept(query, sessionObject.Object);
                if (resGlobalIntercept1 == GlobalInterceptResult.Freeze)
                    return  CommandExecutionResult.AheadFluent(this, sessionObject, entryState.DurationInSec);

                if (entryState.GetStateType() == StateType.Finish)
                {
                    return await entryState.Finalize(query, sessionObject.Object);
                }
                
                await entryState.SendMessages(query, sessionObject.Object);
                return entryState.NeedAnswer
                    ? CommandExecutionResult.AheadFluent(this, sessionObject, entryState.DurationInSec)
                    : CommandExecutionResult.Break();
            }

            if (!sessionObject.CurrentStateId.IsInit)
            {
                var (obj, nextState) = await Entry(query, sessionObject.Object);
                sessionObject.Object = obj;
                sessionObject.CurrentStateId.Data = nextState;
                sessionObject.CurrentStateId.IsInit = true;
                var entryState = stateMachine.GetStateInternal(sessionObject.CurrentStateId.Data);
                var resGlobalIntercept2 = await GlobalIntercept(query, sessionObject.Object);
                if (resGlobalIntercept2 == GlobalInterceptResult.Freeze)
                    return CommandExecutionResult.AheadFluent(this, sessionObject, entryState.DurationInSec);
                if (sessionObject.FireType == FireType.Entry)
                {
                    if (entryState.GetStateType() == StateType.Finish)
                    {
                        return await entryState.Finalize(query, sessionObject.Object);
                    }
                    
                    await entryState.SendMessages(query, sessionObject.Object);
                    return entryState.NeedAnswer
                        ? CommandExecutionResult.AheadFluent(this, sessionObject, entryState.DurationInSec)
                        : CommandExecutionResult.Break();
                }
            }

            var currentState = stateMachine.GetStateInternal(sessionObject.CurrentStateId.Data);
            var resGlobalIntercept = await GlobalIntercept(query, sessionObject.Object);
            if (resGlobalIntercept == GlobalInterceptResult.Freeze)
                return CommandExecutionResult.AheadFluent(this, sessionObject, currentState.DurationInSec);
            
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
            return next.NeedAnswer
                ? CommandExecutionResult.AheadFluent(this, sessionObject, next.DurationInSec)
                : CommandExecutionResult.Break();
        }

        private StateMachine<TObject, TStates> GetStateMachine()
        {
            var builderStateMachine = new StateMachineBuilder<TObject, TStates>();
            return (StateMachine<TObject, TStates>)StateMachine(builderStateMachine);
        }


        public async Task<ICommandExecutionResult> Execute<TQuery>(IQueryCommand<TQuery> currentCommand,
            IQueryContext<TQuery> query, FluentObject<TObject, TStates> sessionObject)
        {
            var stateMachine = StateMachineInternal;
            if (sessionObject.CurrentStateId == null)
                return await DefaultExecute(query, sessionObject);
            var currentState = stateMachine.GetStateInternal(sessionObject.CurrentStateId.Data);
            
            var resGlobalIntercept = await GlobalIntercept(query, sessionObject.Object);
            if (resGlobalIntercept == GlobalInterceptResult.Freeze)
                return CommandExecutionResult.AheadFluent(this, sessionObject, currentState.DurationInSec);
            
            var interceptResult = TryQueryCommandIntercept(sessionObject.CurrentStateId.Data, currentCommand, query,
                sessionObject.Object);
            if (interceptResult.MustIntercept || interceptResult.MustInterceptWithoutExecute)
            {
                if (!interceptResult.MustInterceptWithoutExecute)
                {
                    var executionResult = await currentCommand.Execute(query);
                    if (interceptResult.IsTerminalResult)
                        return executionResult;
                    if (interceptResult.ResultCallback != null)
                    {
                        await interceptResult.ResultCallback(query, sessionObject.Object, executionResult);
                    }
                }

                return CommandExecutionResult.AheadFluent(this, sessionObject, currentState.DurationInSec);
            }
            
            if (await currentState.IsCommandHandle(sessionObject.Object, currentCommand))
            {
                return await currentCommand.Execute(query);
            }

            return await DefaultExecute(query, sessionObject);
        }

        protected virtual InterceptResult<TQuery, TObject> TryQueryCommandIntercept<TQuery>(TStates state,
            IQueryCommand<TQuery> currentCommand, IQueryContext<TQuery> query,
            TObject sessionObject)
        {
            return new InterceptResult<TQuery, TObject>
            {
                IsTerminalResult = false,
                MustIntercept = false,
                ResultCallback = null,
                MustInterceptWithoutExecute = false
            };
        }

        protected virtual Task<GlobalInterceptResult> GlobalIntercept<TQuery>(IQueryContext<TQuery> query,
            TObject sessionObject)
        {
            return Task.FromResult(GlobalInterceptResult.Next);
        }

        public async Task<ICommandExecutionResult> Execute<TQuery>(
            ISessionCommand<TQuery, FluentObject<TObject, TStates>> currentCommand, IQueryContext<TQuery> query,
            FluentObject<TObject, TStates> sessionObject)
        {
            return await DefaultExecute(query, sessionObject);
        }

        protected abstract Task<(TObject, TStates)> Entry<TQuery>(IQueryContext<TQuery> query, TObject currentObject);

        protected abstract IStateMachine<TStates> StateMachine(IStateMachineBuilder<TObject, TStates> builder);
    }
}
