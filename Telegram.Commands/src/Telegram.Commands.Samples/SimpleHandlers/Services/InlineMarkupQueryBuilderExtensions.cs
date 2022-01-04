using System.Collections.Generic;
using SimpleHandlers.Services.Commands;
using SimpleHandlers.Services.Commands.Models;
using Telegram.Commands.Core.Services;

namespace SimpleHandlers.Services
{
    public static class InlineMarkupQueryBuilderExtensions
    {
        public static InlineMarkupQueryBuilder AddMenu(this InlineMarkupQueryBuilder builder)
        {
            builder.AddInlineKeyboardButton<NextPhotoCallback, List<PhotoSessionObject>>(new CallbackData
            {
                Text = "Next photo",
                CallbackText = "this is callback text"
            });
            builder.AddInlineKeyboardButton<ShowResultCallback, List<PhotoSessionObject>>(new CallbackData
            {
                Text = "Show result",
                CallbackText = "this is callback text"
            });
            builder.AddInlineKeyboardButton<CancelCallback>(new CallbackData
            {
                Text = "Cancel",
                CallbackText = ""
            });

            return builder;
        }
    }
}