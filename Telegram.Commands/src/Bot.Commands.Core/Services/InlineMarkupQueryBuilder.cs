using System;
using System.Collections.Generic;
using System.Linq;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Abstract.Messages;

namespace Bot.Commands.Core.Services;

/// <summary>Owns common callback metadata and row layout, independently of any SDK.</summary>
public class InlineMarkupQueryBuilder : IInlineMarkupQueryBuilder
{
    private readonly List<CallbackDataWithCommand[]> _rows = new();

    public IInlineMarkupQueryBuilder InlineKeyboardButtonsRow(CallbackDataWithCommand[] row)
    {
        AddRows(new[] { row });
        return this;
    }

    /// <summary>Returns a snapshot of the keyboard, preserving row boundaries and empty rows.</summary>
    public CallbackDataWithCommand[][] GetCallbacks()
        => _rows.Select(row => row.ToArray()).ToArray();

    protected IEnumerable<CallbackDataWithCommand[]> GetRows() => _rows;

    protected virtual void ValidateRow(CallbackDataWithCommand[] row) { }

    protected virtual void ValidateButtons(CallbackData[] callbacks, ICommandDescriptor descriptor) { }

    protected void AddButtons(CallbackData[] callbacks, ICommandDescriptor descriptor = null)
    {
        ValidateButtons(callbacks, descriptor);
        AddRows(callbacks.Select(callback => new[] { WithDescriptor(callback, descriptor) }).ToArray());
    }

    protected static CallbackDataWithCommand WithDescriptor(CallbackData callback, ICommandDescriptor descriptor)
        => new()
        {
            Text = callback.Text, CallbackText = callback.CallbackText, CallbackMode = callback.CallbackMode,
            CommandDescriptor = descriptor ?? (callback as CallbackDataWithCommand)?.CommandDescriptor
        };

    private void AddRows(CallbackDataWithCommand[][] rows)
    {
        // Validate the entire operation before changing the builder.
        var normalized = rows.Select(row => row.ToArray()).ToArray();
        foreach (var row in normalized) ValidateRow(row);
        _rows.AddRange(normalized);
    }

    public IInlineMarkupQueryBuilder AddInlineKeyboardButton<TCommand, TQuery>(CallbackData callbackData)
        where TCommand : IQueryCommand<TQuery>
        => AddInlineKeyboardButtons<TCommand, TQuery>(new[] { callbackData });

    public IInlineMarkupQueryBuilder AddInlineKeyboardButton<TCommand, TSessionObject, TQuery>(CallbackData callbackData)
        where TCommand : ISessionCommand<TQuery, TSessionObject>
        => AddInlineKeyboardButtons<TCommand, TSessionObject, TQuery>(new[] { callbackData });

    public IInlineMarkupQueryBuilder AddInlineKeyboardButton(CallbackData callbackData)
        => AddInlineKeyboardButton(new[] { callbackData });

    public IInlineMarkupQueryBuilder AddInlineKeyboardButton(CallbackDataWithCommand callbackData)
        => InlineKeyboardButtonsRow(new[] { callbackData });

    public IInlineMarkupQueryBuilder AddInlineKeyboardButton(CallbackData[] callbacks)
    {
        AddButtons(callbacks);
        return this;
    }

    public IInlineMarkupQueryBuilder AddInlineKeyboardButtons<TCommand, TQuery>(CallbackData[] callbacks)
        where TCommand : IQueryCommand<TQuery>
    {
        AddButtons(callbacks, BotCommandExtensions.GetCommandInfo(typeof(TCommand)));
        return this;
    }

    public IInlineMarkupQueryBuilder AddInlineKeyboardButtons<TCommand, TSessionObject, TQuery>(CallbackData[] callbacks)
        where TCommand : ISessionCommand<TQuery, TSessionObject>
    {
        AddButtons(callbacks, BotCommandExtensions.GetCommandInfo(typeof(TCommand)));
        return this;
    }
}
