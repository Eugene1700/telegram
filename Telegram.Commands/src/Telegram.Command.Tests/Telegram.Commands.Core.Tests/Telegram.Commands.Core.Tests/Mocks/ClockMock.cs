using Telegram.Commands.Abstract.Interfaces;

namespace Telegram.Commands.Core.Tests.Mocks;

public class ClockMock : IClock
{
    private DateTime? _now;

    public void SetNow(DateTime dateTime)
    {
        _now = dateTime;
    }

    public DateTime Now => _now ?? DateTime.Now;
}