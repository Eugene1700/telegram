using System.Linq;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Abstract.Messages;

namespace Bot.Commands.Core.Services;

/// <summary>Shared typed adapter API. Only native button and markup conversion belong to a messenger.</summary>
public abstract class AdapterInlineMarkupQueryBuilder<TBuilder, TQuery, TButton, TMarkup> : InlineMarkupQueryBuilder
    where TBuilder : AdapterInlineMarkupQueryBuilder<TBuilder, TQuery, TButton, TMarkup>
{
    private TBuilder Self => (TBuilder)this;
    protected abstract TButton ToButton(CallbackDataWithCommand callback);
    protected abstract TMarkup ToMarkup(TButton[][] rows);
    protected virtual void ValidateButtons(CallbackData[] callbacks, ICommandDescriptor descriptor) { }

    protected override void ValidateRow(CallbackDataWithCommand[] row)
    {
        // Keep conversion errors at insertion time, as in the original adapter builders.
        foreach (var callback in row) ToButton(callback);
    }

    public TBuilder AddInlineKeyboardButtons<TCommand>(CallbackData[] callbacks)
        where TCommand : IQueryCommand<TQuery>
        => AddNativeButtons(callbacks, BotCommandExtensions.GetCommandInfo<TCommand, TQuery>());

    public TBuilder AddInlineKeyboardButtons<TCommand, TSessionObject>(CallbackData[] callbacks)
        where TCommand : ISessionCommand<TQuery, TSessionObject>
        => AddNativeButtons(callbacks, BotCommandExtensions.GetCommandInfo<TCommand, TQuery, TSessionObject>());

    public TBuilder AddInlineKeyboardButton<TCommand>(CallbackData callback)
        where TCommand : IQueryCommand<TQuery> => AddInlineKeyboardButtons<TCommand>(new[] { callback });

    public new TBuilder AddInlineKeyboardButton<TCommand, TSessionObject>(CallbackData callback)
        where TCommand : ISessionCommand<TQuery, TSessionObject>
        => AddInlineKeyboardButtons<TCommand, TSessionObject>(new[] { callback });

    public TBuilder AddInlineKeyboardButton(CallbackData[] callbacks) => AddNativeButtons(callbacks, null);
    public TBuilder AddInlineKeyboardButton(CallbackData callback) => AddInlineKeyboardButton(new[] { callback });
    public TBuilder AddInlineKeyboardButton(CallbackDataWithCommand callback) => InlineKeyboardButtonsRow(new[] { callback });

    public new TBuilder InlineKeyboardButtonsRow(CallbackDataWithCommand[] callbacks)
    {
        base.InlineKeyboardButtonsRow(callbacks);
        return Self;
    }

    public TMarkup GetResult() => ToMarkup(GetRows().Select(row => row.Select(ToButton).ToArray()).ToArray());

    private TBuilder AddNativeButtons(CallbackData[] callbacks, ICommandDescriptor descriptor)
    {
        ValidateButtons(callbacks, descriptor);
        AddButtons(callbacks, descriptor);
        return Self;
    }
}
