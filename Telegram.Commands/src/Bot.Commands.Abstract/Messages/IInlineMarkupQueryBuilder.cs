using Bot.Commands.Abstract.Commands;

namespace Bot.Commands.Abstract.Messages
{
    public interface IInlineMarkupQueryBuilder
    {
        IInlineMarkupQueryBuilder InlineKeyboardButtonsRow(CallbackDataWithCommand[] row);

        CallbackDataWithCommand[][] GetCallbacks();

        IInlineMarkupQueryBuilder AddInlineKeyboardButton(CallbackData callbackData);
        IInlineMarkupQueryBuilder AddInlineKeyboardButton(CallbackDataWithCommand callbackData);

        /// <summary>Adds each button on its own row.</summary>
        IInlineMarkupQueryBuilder AddInlineKeyboardButton(CallbackData[] callbacks);

        IInlineMarkupQueryBuilder AddInlineKeyboardButtons<TCommand, TQuery>(CallbackData[] callbacks)
            where TCommand : IQueryCommand<TQuery>;

        IInlineMarkupQueryBuilder AddInlineKeyboardButtons<TCommand, TSessionObject, TQuery>(CallbackData[] callbacks)
            where TCommand : ISessionCommand<TQuery, TSessionObject>;
        
        IInlineMarkupQueryBuilder AddInlineKeyboardButton<TCommand, TQuery>(CallbackData callbackData)
            where TCommand : IQueryCommand<TQuery>;

        IInlineMarkupQueryBuilder
            AddInlineKeyboardButton<TCommand, TSessionObject, TQuery>(CallbackData callbackData)
            where TCommand : ISessionCommand<TQuery, TSessionObject>;
        
    }
}
