using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Commands.Abstract.Commands;

namespace Telegram.Commands.Abstract.Interfaces
{
    public interface IInlineMarkupQueryBuilder
    {
        IInlineMarkupQueryBuilder AddInlineKeyboardButtons<TCommand>(ICallbackData[] callbackQueries)
            where TCommand : IQueryTelegramCommand<CallbackQuery>;

        IInlineMarkupQueryBuilder AddInlineKeyboardButtons<TCommand, TSessionObject>(ICallbackData[] callbackQueries)
            where TCommand : ISessionTelegramCommand<CallbackQuery, TSessionObject>;

        IInlineMarkupQueryBuilder AddInlineKeyboardButton<TCommand>(ICallbackData callbackData)
            where TCommand : IQueryTelegramCommand<CallbackQuery>;

        IInlineMarkupQueryBuilder AddInlineKeyboardButton<TCommand, TSessionObject>(ICallbackData callbackData)
            where TCommand : ISessionTelegramCommand<CallbackQuery, TSessionObject>;

        IInlineMarkupQueryBuilder AddInlineKeyboardButton(ICallbackData[] callbackQueries);

        IInlineMarkupQueryBuilder AddInlineKeyboardButton(ICallbackData callbackData);

        IInlineMarkupQueryBuilder InlineKeyboardButtonsRow(ICallbackDataWithCommand[] callbackData);

        IInlineMarkupQueryBuilder AddInlineKeyboardButton(ICallbackDataWithCommand callbackData);

        InlineKeyboardMarkup GetInlineKeyboardMarkup();

        IReplyMarkup GetResult();
    }

    public interface ICallbackDataWithCommand: ICallbackData
    {
        ITelegramCommandDescriptor CommandDescriptor { get; }
    }

    public interface ICallbackData
    {
        string Text { get; }
        string CallbackText { get;}
        CallbackMode CallbackMode { get; }
    }
    
    public enum CallbackMode
    {
        CallbackData = 0,
        InlineCurrent = 1,
        Inline = 2,
        Url = 3,
        Payment = 4
    }
}