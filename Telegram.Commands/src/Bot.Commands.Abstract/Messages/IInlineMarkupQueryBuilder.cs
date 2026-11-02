using System.Collections.ObjectModel;

namespace Bot.Commands.Abstract.Messages
{
    public interface IInlineMarkupQueryBuilder
    {
        IInlineMarkupQueryBuilder InlineKeyboardButtonsRow(CallbackDataWithCommand[] row);

        ReadOnlyCollection<CallbackDataWithCommand> GetCallbacks();
    }
}