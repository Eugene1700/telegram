using Telegram.Bot;
using Telegram.Commands.Abstract.Interfaces;

namespace Telegram.Commands.Core.Models
{
    public class TelegramClient: TelegramBotClient
    {
        private ITelegramBotProfile _telegramBotProfile;

        public TelegramClient(ITelegramBotProfile telegramBotProfile) : base(telegramBotProfile.Key)
        {
            _telegramBotProfile = telegramBotProfile;
            var hook = $"{telegramBotProfile.BaseUrl}/{telegramBotProfile.UpdateRoute}";
            SetWebhookAsync(hook);
        }
    }
}