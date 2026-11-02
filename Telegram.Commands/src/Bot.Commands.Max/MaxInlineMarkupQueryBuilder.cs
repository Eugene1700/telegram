using global::Max.Bot.Types;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Abstract.Messages;
using Bot.Commands.Core;
using Bot.Commands.Max.Models;

namespace Bot.Commands.Max;

/// <summary>Builds native Max inline keyboards. Array overloads add one button per row.</summary>
public class MaxInlineMarkupQueryBuilder
{
    private readonly List<InlineKeyboardButton[]> _buttons = new();

    public MaxInlineMarkupQueryBuilder AddInlineKeyboardButtons<TCommand>(CallbackData[] callbacks)
        where TCommand : IQueryCommand<CallbackQuery>
        => AddButtons(callbacks, BotCommandExtensions.GetCommandInfo<TCommand, CallbackQuery>());

    public MaxInlineMarkupQueryBuilder AddInlineKeyboardButtons<TCommand, TSessionObject>(CallbackData[] callbacks)
        where TCommand : ISessionCommand<CallbackQuery, TSessionObject>
        => AddButtons(callbacks, BotCommandExtensions.GetCommandInfo<TCommand, CallbackQuery, TSessionObject>());

    public MaxInlineMarkupQueryBuilder AddInlineKeyboardButton<TCommand>(CallbackData callback)
        where TCommand : IQueryCommand<CallbackQuery>
        => AddInlineKeyboardButtons<TCommand>(new[] { callback });

    public MaxInlineMarkupQueryBuilder AddInlineKeyboardButton<TCommand, TSessionObject>(CallbackData callback)
        where TCommand : ISessionCommand<CallbackQuery, TSessionObject>
        => AddInlineKeyboardButtons<TCommand, TSessionObject>(new[] { callback });

    public MaxInlineMarkupQueryBuilder AddInlineKeyboardButton(CallbackData[] callbacks) => AddButtons(callbacks, null);
    public MaxInlineMarkupQueryBuilder AddInlineKeyboardButton(CallbackData callback) => AddInlineKeyboardButton(new[] { callback });
    public MaxInlineMarkupQueryBuilder AddInlineKeyboardButton(CallbackDataWithCommand callback)
        => InlineKeyboardButtonsRow(new[] { callback });

    public MaxInlineMarkupQueryBuilder InlineKeyboardButtonsRow(CallbackDataWithCommand[] callbacks)
    {
        _buttons.Add(callbacks.Select(MaxMessageExtensions.ToButton).ToArray());
        return this;
    }

    public InlineKeyboard GetResult() => new(_buttons.ToArray());

    private MaxInlineMarkupQueryBuilder AddButtons(CallbackData[] callbacks, ICommandDescriptor descriptor)
    {
        var rows = callbacks.Select(callback => new[] { MaxMessageExtensions.ToButton(new CallbackDataWithCommand
        {
            Text = callback.Text, CallbackText = callback.CallbackText, CallbackMode = callback.CallbackMode,
            CommandDescriptor = descriptor ?? (callback as CallbackDataWithCommand)?.CommandDescriptor
        }) }).ToArray();
        _buttons.AddRange(rows);
        return this;
    }
}
