using System.Collections.Generic;
using Telegram.Bot.Types;
using Telegram.Commands.Abstract.Interfaces.Commands;
using Telegram.Commands.Core;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.UI.Pagination
{
    public static class PaginationMenu
    {
        public static InlineMarkupQueryBuilder AddPaginationMenu<TMoveCommand, TSessionObject>(this InlineMarkupQueryBuilder builder, IPaginationMenu pagination) where TMoveCommand : ISessionTelegramCommand<CallbackQuery, TSessionObject>
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
                        CommandDescriptor = TelegramCommandExtensions.GetCommandInfo<TMoveCommand, CallbackQuery, TSessionObject>()
                    });
                }

                if (pagination.IsSecondOrMorePage())
                {
                    lstPaginationComs.Add(new CallbackDataWithCommand
                    {
                        CallbackText = $"{pagination.PreviousPageNumber()}",
                        Text = $"{pagination.PreviousPageNumber()} <",
                        CommandDescriptor = TelegramCommandExtensions.GetCommandInfo<TMoveCommand, CallbackQuery, TSessionObject>()
                    });
                }
            }

            lstPaginationComs.Add(new CallbackDataWithCommand
            {
                CallbackText = $"{pagination.PageNumber}",
                Text = $"·{pagination.PageNumber}·",
                CommandDescriptor = TelegramCommandExtensions.GetCommandInfo<TMoveCommand, CallbackQuery, TSessionObject>()
            });

            if (!pagination.IsLastPage())
            {
                lstPaginationComs.Add(new CallbackDataWithCommand
                {
                    CallbackText = $"{pagination.NextPageNumber()}",
                    Text = $"> {pagination.NextPageNumber()}",
                    CommandDescriptor = TelegramCommandExtensions.GetCommandInfo<TMoveCommand, CallbackQuery, TSessionObject>()
                });

                if (!pagination.IsPenultPage())
                {
                    lstPaginationComs.Add(new CallbackDataWithCommand
                    {
                        CallbackText = $"{pagination.PagesCount()}",
                        Text = $">> {pagination.PagesCount()}",
                        CommandDescriptor = TelegramCommandExtensions.GetCommandInfo<TMoveCommand, CallbackQuery, TSessionObject>()
                    });
                }
            }

            builder.InlineKeyboardButtonsRow(lstPaginationComs.ToArray());
            return builder;
        }
    }
}