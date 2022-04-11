using System.Threading.Tasks;

namespace Telegram.Commands.Abstract.Interfaces.Commands
{
    public interface IQueryCommand<in TQuery> : IBaseCommand<TQuery, EmptyObject, EmptyObject>
    {
    }
    
    public interface ISessionDataCommand<in TQuery, in TSessionData> : IBaseCommand<TQuery, EmptyObject, TSessionData>
    {
    }
    
    public interface IQueryDataCommand<in TQuery, in TQueryData> : IBaseCommand<TQuery, TQueryData, EmptyObject>
    {
    }

    public interface IBaseCommand<in TQuery, in TQueryData, in TSessionData>
    {
        Task<ITelegramCommandExecutionResult> Execute(TQuery query, TQueryData queryData, TSessionData sessionData);
    }
}