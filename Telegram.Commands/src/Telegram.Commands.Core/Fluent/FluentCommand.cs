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
    public const string DefaultNextCondition = "default_condition";

    public async Task<ITelegramCommandExecutionResult> DefaultExecute<TQuery>(TQuery query,
        FluentObject<TObject> sessionObject)
    {
        var stateMachine = GetStateMachine();
        if (sessionObject == null)
        {
            sessionObject = new FluentObject<TObject>(await Entry(query))
            {
                CurrentStateId = 0
            };
            var entryState = stateMachine.GetCurrentStateInternal(sessionObject.CurrentStateId.Value);
            var nextMessage = entryState.GetMessage();
            await SendMessage(query, nextMessage);
            return TelegramCommandExecutionResult.AheadFluent(this, sessionObject, null);
        }

        var currentState = stateMachine.GetCurrentStateInternal(sessionObject.CurrentStateId.Value);
        var condition = query switch
        {
            Message message => await currentState.Commit(message.Text, sessionObject.Object),
            CallbackQuery callbackQuery => await currentState.CallbackCommit(callbackQuery.Data, sessionObject.Object),
            _ => throw new InvalidOperationException("FluentCommand works only with Message and CallbackQuery")
        };

        var next = currentState.Next(condition);
        if (next == null)
        {
            return await Finalize(query, sessionObject.Object);
        }

        if (next.Id != currentState.Id)
        {
            var nextMessage = next.GetMessage();
            await SendMessage(query, nextMessage);
        }

        sessionObject.CurrentStateId = next.Id;
        return TelegramCommandExecutionResult.AheadFluent(this, sessionObject, null);
    }

    private StateMachine<TObject> GetStateMachine()
    {
        var builderStateMachine = new StateMachineBuilder<TObject>();
        var stateMachine = (StateMachine<TObject>)StateMachine(builderStateMachine);
        return stateMachine;
    }

    protected abstract Task SendMessage<TQuery>(TQuery currentQuery, ITelegramMessage nextMessage);

    public async Task<ITelegramCommandExecutionResult> Execute<TQuery>(IQueryTelegramCommand<TQuery> currentCommand,
        TQuery query, FluentObject<TObject> sessionObject)
    {
        var stateMachine = GetStateMachine();
        if (sessionObject.CurrentStateId == null) 
            return await DefaultExecute(query, sessionObject);
        
        var currentState = stateMachine.GetCurrentStateInternal(sessionObject.CurrentStateId.Value);
        if (currentState.CanNext(currentCommand as IQueryTelegramCommand<CallbackQuery>))
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

    protected abstract Task<TObject> Entry<TQuery>(TQuery query);
    protected abstract IStateMachine<TObject> StateMachine(IStateMachineBuilder<TObject> builder);
    protected abstract Task<ITelegramCommandExecutionResult> Finalize<TQuery>(TQuery currentQuery, TObject obj);
}