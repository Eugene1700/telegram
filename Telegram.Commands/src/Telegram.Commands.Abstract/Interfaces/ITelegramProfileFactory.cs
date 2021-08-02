using System;
using System.Threading.Tasks;

namespace Telegram.Commands.Abstract.Interfaces
{
    public interface ITelegramProfileFactory
    {
        ITelegramBotProfile GetProfile(Type profileType);
    }
}