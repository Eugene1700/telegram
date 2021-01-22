using System.Threading.Tasks;
using Telegram.Bot.Types.Payments;

namespace Telegram.Commands.Abstract.Interfaces
{
    public interface IPaymentProvider
    {
        Task HandlePreCheckoutQuery(PreCheckoutQuery updatePreCheckoutQuery);
        Task HandleSuccessfulPayment(SuccessfulPayment updatePreCheckoutQuery);
    }
}