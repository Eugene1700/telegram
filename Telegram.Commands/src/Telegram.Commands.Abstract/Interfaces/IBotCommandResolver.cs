using System.Threading.Tasks;

namespace Telegram.Commands.Abstract.Interfaces
{
    public interface IBotCommandResolver<in TUpdate>
    {
        Task Handle(TUpdate update);
    }
}