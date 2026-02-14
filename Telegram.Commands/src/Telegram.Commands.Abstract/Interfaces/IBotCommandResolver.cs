using System.Threading.Tasks;

namespace Telegram.Commands.Abstract.Interfaces
{
    public interface IBotCommandResolver<in TUpdate, TResult>
    {
        Task<TResult> Handle(TUpdate update);
    }
}