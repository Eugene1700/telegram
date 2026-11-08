using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

    /// <summary>Returns a snapshot of the current buttons in row order.</summary>
    public ReadOnlyCollection<CallbackDataWithCommand> GetCallbacks()
        => Array.AsReadOnly(_rows.SelectMany(row => row).ToArray());

    protected IEnumerable<CallbackDataWithCommand[]> GetRows() => _rows;

    protected virtual void ValidateRow(CallbackDataWithCommand[] row) { }

    protected void AddButtons(CallbackData[] callbacks, ICommandDescriptor descriptor = null)
    {
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
        var normalized = rows.Select((row, index) =>
            row.Select(callback => callback with { Row = _rows.Count + index }).ToArray()).ToArray();
        foreach (var row in normalized) ValidateRow(row);
        _rows.AddRange(normalized);
    }

    public IInlineMarkupQueryBuilder AddInlineKeyboardButton<TCommand, TQuery>(CallbackData callbackData)
        where TCommand : IQueryCommand<TQuery>
    {
        AddButtons(new[] { callbackData }, BotCommandExtensions.GetCommandInfo(typeof(TCommand)));
        return this;
    }

    public IInlineMarkupQueryBuilder AddInlineKeyboardButton<TCommand, TSessionObject, TQuery>(CallbackData callbackData)
        where TCommand : ISessionCommand<TQuery, TSessionObject>
    {
        AddButtons(new[] { callbackData }, BotCommandExtensions.GetCommandInfo(typeof(TCommand)));
        return this;
    }
}
