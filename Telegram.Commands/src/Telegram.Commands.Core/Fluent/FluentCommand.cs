using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Abstract.Interfaces.Commands;
using Telegram.Commands.Core.Fluent.Builders;
using Telegram.Commands.Core.Fluent.StateMachine;
using Telegram.Commands.Core.Models;

namespace Telegram.Commands.Core.Fluent;

public abstract class FluentCommand<TObject> : IBehaviorTelegramCommand<FluentObject<TObject>>
{

    public async Task<ITelegramCommandExecutionResult> DefaultExecute<TQuery>(TQuery query,
        FluentObject<TObject> sessionObject)
    {
        var stateMachine = GetStateMachine();
        if (sessionObject == null)
        {
            sessionObject = new FluentObject<TObject>(await Entry(query, default))
            {
                CurrentStateId = stateMachine.GetEntryState().Id
            };
            var entryState = stateMachine.GetStateInternal(sessionObject.CurrentStateId);
            await entryState.SendMessage(query, sessionObject.Object);
            return TelegramCommandExecutionResult.AheadFluent(this, sessionObject, entryState.DurationInSec);
        }

        if (string.IsNullOrWhiteSpace(sessionObject.CurrentStateId))
        {
            var d = await Entry(query, sessionObject.Object);
            sessionObject.Object = d;
            sessionObject.CurrentStateId = stateMachine.GetEntryState().Id;
        }
        
        var currentState = stateMachine.GetStateInternal(sessionObject.CurrentStateId);
        var nextStateId = await currentState.HandleQuery(query, sessionObject.Object);

        var next = stateMachine.GetStateInternal(nextStateId);
        if (next.GetStateType() == StateType.Finish)
        {
            return await Finalize(query, sessionObject.Object);
        }

        if (next.Id != currentState.Id)
        {
            await next.SendMessage(query, sessionObject.Object);
        }

        sessionObject.CurrentStateId = next.Id;
        return TelegramCommandExecutionResult.AheadFluent(this, sessionObject, next.DurationInSec);
    }

    private StateMachine<TObject> GetStateMachine()
    {
        var builderStateMachine = new StateMachineBuilder<TObject>();
        var stateMachine = (StateMachine<TObject>)StateMachine(builderStateMachine);
        return stateMachine;
    }

    
    public async Task<ITelegramCommandExecutionResult> Execute<TQuery>(IQueryTelegramCommand<TQuery> currentCommand,
        TQuery query, FluentObject<TObject> sessionObject)
    {
        var stateMachine = GetStateMachine();
        if (sessionObject.CurrentStateId == null) 
            return await DefaultExecute(query, sessionObject);
        
        var currentState = stateMachine.GetStateInternal(sessionObject.CurrentStateId);
        if (currentState.IsCommandHandle(currentCommand as IQueryTelegramCommand<CallbackQuery>))
        {
            return await currentCommand.Execute(query);
        }

        return await DefaultExecute(query, sessionObject);
    }

    public async Task<ITelegramCommandExecutionResult> Execute<TQuery>(
        ISessionTelegramCommand<TQuery, FluentObject<TObject>> currentCommand, TQuery query,
        FluentObject<TObject> sessionObject)
    {
        return await DefaultExecute(query, sessionObject);
    }

    protected abstract Task<TObject> Entry<TQuery>(TQuery query, TObject currentObject);

    protected abstract IStateMachine<TObject> StateMachine(IStateMachineBuilder<TObject> builder);
    protected abstract Task<ITelegramCommandExecutionResult> Finalize<TQuery>(TQuery currentQuery, TObject obj);

}