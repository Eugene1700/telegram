using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Commands.Abstract.Interfaces;

namespace Telegram.Commands.Core.Models
{
    public class TelegramClient: TelegramBotClient
    {
        private readonly ITelegramBotProfile _telegramBotProfile;

        public TelegramClient(ITelegramBotProfile telegramBotProfile) : base(telegramBotProfile.Key)
        {
            _telegramBotProfile = telegramBotProfile;
        }

        public async Task InitWebhook()
        {
            var hook = $"{_telegramBotProfile.BaseUrl}/{_telegramBotProfile.UpdateRoute}";
            await SetWebhookAsync(hook);
        }
    }
}