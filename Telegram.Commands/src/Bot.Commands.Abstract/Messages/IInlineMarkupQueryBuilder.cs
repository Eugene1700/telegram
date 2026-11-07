using System.Collections.ObjectModel;
using Bot.Commands.Abstract.Commands;

namespace Bot.Commands.Abstract.Messages
{
    public interface IInlineMarkupQueryBuilder
    {
        IInlineMarkupQueryBuilder InlineKeyboardButtonsRow(CallbackDataWithCommand[] row);

        ReadOnlyCollection<CallbackDataWithCommand> GetCallbacks();
        
        IInlineMarkupQueryBuilder AddInlineKeyboardButton<TCommand, TQuery>(CallbackData callbackData)
            where TCommand : IQueryCommand<TQuery>;

        IInlineMarkupQueryBuilder
            AddInlineKeyboardButton<TCommand, TSessionObject, TQuery>(CallbackData callbackData)
            where TCommand : ISessionCommand<TQuery, TSessionObject>;
        
    }
}