using System.Threading.Tasks;
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
        }

        public static async Task InitWebhook(ITelegramBotProfile telegramBotProfile)
        {
            var client = new TelegramBotClient(telegramBotProfile.Key);
            var hook = $"{telegramBotProfile.BaseUrl}/{telegramBotProfile.UpdateRoute}";
            await client.SetWebhookAsync(hook);
        }
    }
}