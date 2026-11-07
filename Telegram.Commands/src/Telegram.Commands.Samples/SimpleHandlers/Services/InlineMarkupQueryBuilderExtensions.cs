using Bot.Commands.Abstract.Messages;
using Bot.Commands.Core;
using Bot.Commands.Core.Services;
using SimpleHandlers.Services.Commands;
namespace SimpleHandlers.Services;
public static class InlineMarkupQueryBuilderExtensions
{
    public static InlineMarkupQueryBuilder AddCommand<T>(this InlineMarkupQueryBuilder builder, string text, string data = "")
    {
        builder.InlineKeyboardButtonsRow(new[] { new CallbackDataWithCommand { Text = text, CallbackText = data,
            CommandDescriptor = BotCommandExtensions.GetCommandInfo(typeof(T)) } });
        return builder;
    }
    public static InlineMarkupQueryBuilder AddMenu(this InlineMarkupQueryBuilder builder) => builder
        .AddCommand<NextPhotoCallback>("Next photo").AddCommand<ShowResultCallback>("Show result").AddCommand<CancelCallback>("Cancel");
}
