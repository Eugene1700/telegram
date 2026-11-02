using global::Telegram.Bot.Types;
using System.Collections.Generic;
using SimpleHandlers.Services.Commands;
using SimpleHandlers.Services.Commands.Models;
using Bot.Commands.Abstract.Messages;
using Bot.Commands.Core.Services;

namespace SimpleHandlers.Services
{
    public static class InlineMarkupQueryBuilderExtensions
    {
        public static InlineMarkupQueryBuilder AddMenu(this InlineMarkupQueryBuilder builder)
        {
            builder.AddInlineKeyboardButton<NextPhotoCallback, List<PhotoSessionObject>, CallbackQuery>(new CallbackData
            {
                Text = "Next photo",
                CallbackText = "this is callback text"
            });
            builder.AddInlineKeyboardButton<ShowResultCallback, List<PhotoSessionObject>, CallbackQuery>(new CallbackData
            {
                Text = "Show result",
                CallbackText = "this is callback text"
            });
            builder.AddInlineKeyboardButton<CancelCallback, CallbackQuery>(new CallbackData
            {
                Text = "Cancel",
                CallbackText = ""
            });

            return builder;
        }
    }
}