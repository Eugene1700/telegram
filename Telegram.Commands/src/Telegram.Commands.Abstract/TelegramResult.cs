namespace Telegram.Commands.Abstract
{
    public class TelegramResult
    {
        public bool Ok { get; }

        public TelegramResult(bool ok)
        {
            Ok = ok;
        }
    }
}