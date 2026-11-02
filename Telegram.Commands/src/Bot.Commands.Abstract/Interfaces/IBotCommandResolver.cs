using System.Threading.Tasks;

namespace Bot.Commands.Abstract.Interfaces
{
    public interface IBotCommandResolver<in TUpdate>
    {
        Task Handle(TUpdate update);
    }
}