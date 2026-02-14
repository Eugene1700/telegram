using System.Collections.Generic;
using Telegram.Commands.Abstract.Commands;
using Telegram.Commands.Abstract.Messages;
using Telegram.Commands.Core;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.UI.Pagination
{
    public static class PaginationMenu
    {
        public static InlineMarkupQueryBuilder AddPaginationMenu<TMoveCommand, TSessionObject, TQuery>(this InlineMarkupQueryBuilder builder, IPaginationMenu pagination) where TMoveCommand : ISessionTelegramCommand<TQuery, TSessionObject>
        {
            var lstPaginationComs = new List<CallbackDataWithCommand>();
            if (!pagination.IsFirstPage())
            {
                if (pagination.IsThirdOrMorePage())
                {
                    lstPaginationComs.Add(new CallbackDataWithCommand
                    {
                        CallbackText = "1",
                        Text = "1 <<",
                        CommandDescriptor = BotCommandExtensions.GetCommandInfo<TMoveCommand, TQuery, TSessionObject>()
                    });
                }

                if (pagination.IsSecondOrMorePage())
                {
                    lstPaginationComs.Add(new CallbackDataWithCommand
                    {
                        CallbackText = $"{pagination.PreviousPageNumber()}",
                        Text = $"{pagination.PreviousPageNumber()} <",
                        CommandDescriptor = BotCommandExtensions.GetCommandInfo<TMoveCommand, TQuery, TSessionObject>()
                    });
                }
            }

            lstPaginationComs.Add(new CallbackDataWithCommand
            {
                CallbackText = $"{pagination.PageNumber}",
                Text = $"·{pagination.PageNumber}·",
                CommandDescriptor = BotCommandExtensions.GetCommandInfo<TMoveCommand, TQuery, TSessionObject>()
            });

            if (!pagination.IsLastPage())
            {
                lstPaginationComs.Add(new CallbackDataWithCommand
                {
                    CallbackText = $"{pagination.NextPageNumber()}",
                    Text = $"> {pagination.NextPageNumber()}",
                    CommandDescriptor = BotCommandExtensions.GetCommandInfo<TMoveCommand, TQuery, TSessionObject>()
                });

                if (!pagination.IsPenultPage())
                {
                    lstPaginationComs.Add(new CallbackDataWithCommand
                    {
                        CallbackText = $"{pagination.PagesCount()}",
                        Text = $">> {pagination.PagesCount()}",
                        CommandDescriptor = BotCommandExtensions.GetCommandInfo<TMoveCommand, TQuery, TSessionObject>()
                    });
                }
            }

            builder.InlineKeyboardButtonsRow(lstPaginationComs.ToArray());
            return builder;
        }
    }
}