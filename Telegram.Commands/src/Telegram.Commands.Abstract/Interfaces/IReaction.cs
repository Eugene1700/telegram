namespace Telegram.Commands.Abstract.Interfaces
{
    public interface IReaction<TCommand, TCommandQuery, in TSessionObject>
        where TCommand : ISessionTelegramCommand<TCommandQuery, TSessionObject>
    {
        
    }
    
    public interface IReaction<TCommand, TCommandQuery>
        where TCommand : ITelegramCommand<TCommandQuery>
    {
        
    }

    public interface ISessionSessionCommandWithReactions<in TQuery, in TSessionObject> :
        ISessionTelegramCommand<TQuery, TSessionObject>
    {
        bool ReactionCondition<TCurrentQuery>(ITelegramCommandDescriptor telegramCommandDescriptor,
            TCurrentQuery currentQuery);
    }
}