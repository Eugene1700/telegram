using Telegram.Commands.Abstract.Interfaces;

namespace Telegram.Commands.Core.Tests.Mocks;

public class SessionStoreMock : ISessionsStore
{
    private ISessionInfo _sessionInfo;
    private ISessionInfoWithData _sessionInfoWithData;
    private object _sessionInfoWithDataGeneric;
    private Action<ISessionInfoWithData> _createSessionCallback;
    private Action<ISessionInfoWithData,long> _updateSessionCallback;

    public ISessionInfo GetSessionInfo(DateTime now, long chatId, long telegramUserId)
    {
        return _sessionInfo;
    }

    public void SetSessionInfo(ISessionInfo sessionInfo)
    {
        _sessionInfo = sessionInfo;
    }

    public ISessionInfoWithData GetSessionInfoWithData(DateTime now, long chatId, long telegramUserId, string commandQuery,
        Type sessionObject)
    {
        return _sessionInfoWithData;
    }
    
    public void SetSessionInfoWithData(ISessionInfoWithData sessionInfo)
    {
        _sessionInfoWithData = sessionInfo;
    }

    public ISessionInfoWithData<TData> GetSessionInfoWithData<TData>(DateTime now, long chatId, long telegramUserId, string commandQuery)
    {
        return (ISessionInfoWithData<TData>)_sessionInfoWithDataGeneric;
    }
    
    public void SetSessionInfoWithData<TData>(ISessionInfoWithData<TData> sessionInfo)
    {
        _sessionInfoWithDataGeneric = sessionInfo;
    }

    public Task CreateSession(ISessionInfoWithData getCommandQuery)
    {
        _createSessionCallback?.Invoke(getCommandQuery);
        return Task.CompletedTask;
    }
    
    public void SetCreateSessionCallback(Action<ISessionInfoWithData> callback)
    {
        _createSessionCallback = callback;
    }

    public Task UpdateSession(ISessionInfoWithData session, long chatIdFrom)
    {
        _updateSessionCallback?.Invoke(session, chatIdFrom);
        return Task.CompletedTask;
    }
    
    public void SetUpdateSessionCallback(Action<ISessionInfoWithData, long> callback)
    {
        _updateSessionCallback = callback;
    }
}