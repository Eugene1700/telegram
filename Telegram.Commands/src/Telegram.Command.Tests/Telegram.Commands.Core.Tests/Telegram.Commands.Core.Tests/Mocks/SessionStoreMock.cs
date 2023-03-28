using Telegram.Commands.Abstract.Interfaces;

namespace Telegram.Commands.Core.Tests.Mocks;

public class SessionStoreMock : ISessionsStore
{
    private ISessionInfo _sessionInfo;
    private ISessionInfoWithData _sessionInfoWithData;
    private object _sessionInfoWithDataGeneric;
    private Action<ISessionInfoWithData> _createSessionCallback;
    private Action<ISessionInfoWithData, long> _updateSessionCallback;
    private Func<DateTime, long, long, ISessionInfo> _getSessionInfoCallback;
    private Func<DateTime, long, long, string, Type, ISessionInfoWithData> _getSessionInfoWithDataCallback;
    private Func<DateTime, long, long, string, Type, object> _getSessionInfoWithDataGenericCallback;

    public ISessionInfo GetSessionInfo(DateTime now, long chatId, long telegramUserId)
    {
        return _getSessionInfoCallback != null
            ? _getSessionInfoCallback.Invoke(now, chatId, telegramUserId)
            : _sessionInfo;
    }

    public void SetSessionInfo(ISessionInfo sessionInfo)
    {
        _sessionInfo = sessionInfo;
    }

    public void SetSessionInfoCallback(Func<DateTime, long, long, ISessionInfo> getSessionInfoCallback)
    {
        _getSessionInfoCallback = getSessionInfoCallback;
    }

    public ISessionInfoWithData GetSessionInfoWithData(DateTime now, long chatId, long telegramUserId,
        string commandQuery,
        Type sessionObject)
    {
        return _getSessionInfoWithDataCallback != null
            ? _getSessionInfoWithDataCallback.Invoke(now, chatId, telegramUserId, commandQuery, sessionObject)
            : _sessionInfoWithData;
    }

    public void SetSessionInfoWithData(ISessionInfoWithData sessionInfo)
    {
        _sessionInfoWithData = sessionInfo;
    }

    public void SetSessionInfoWithDataCallback(
        Func<DateTime, long, long, string, Type, ISessionInfoWithData> getSessionInfoWithDataCallback)
    {
        _getSessionInfoWithDataCallback = getSessionInfoWithDataCallback;
    }

    public ISessionInfoWithData<TData> GetSessionInfoWithData<TData>(DateTime now, long chatId, long telegramUserId,
        string commandQuery)
    {
        return _getSessionInfoWithDataGenericCallback != null
            ? (ISessionInfoWithData<TData>)_getSessionInfoWithDataGenericCallback.Invoke(now, chatId, telegramUserId, commandQuery, typeof(TData))
            : (ISessionInfoWithData<TData>)_sessionInfoWithDataGeneric;
    }

    public void SetSessionInfoWithData<TData>(ISessionInfoWithData<TData> sessionInfo)
    {
        _sessionInfoWithDataGeneric = sessionInfo;
    }

    public void SetSessionInfoWithDataCallback<TData>(Func<DateTime, long, long, string, Type, object> callback)
    {
        _getSessionInfoWithDataGenericCallback = callback;
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