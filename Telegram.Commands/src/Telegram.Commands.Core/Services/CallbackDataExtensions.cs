using Telegram.Commands.Abstract.Messages;

namespace Telegram.Commands.Core.Services
{
    internal static class CallbackDataExtensions
    {
        public static bool IsInline(this CallbackMode mode)
        {
            return mode is CallbackMode.Inline or CallbackMode.InlineCurrent;
        }
    }
}