using System.Collections.ObjectModel;

namespace Telegram.Commands.Abstract.Messages
{
    public interface IInlineMarkupQueryBuilder
    {
        IInlineMarkupQueryBuilder InlineKeyboardButtonsRow(CallbackDataWithCommand[] row);

        ReadOnlyCollection<CallbackDataWithCommand> GetCallbacks();
    }
}