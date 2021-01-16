using System;
using System.IO;
using System.Threading.Tasks;
using Telegram.Bot;

namespace Telegram.Commands.Core
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