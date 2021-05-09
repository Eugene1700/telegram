namespace Telegram.Commands.Core.Services
{
    internal static class CallbackDataExtensions
    {
        public static bool IsInline(this CallbackMode mode)
        {
            return mode == CallbackMode.Inline || mode == CallbackMode.InlineCurrent;
        }
    }
}