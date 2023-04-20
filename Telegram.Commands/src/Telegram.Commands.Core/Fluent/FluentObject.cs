namespace Telegram.Commands.Core.Fluent;

public class FluentObject<TObject, TStates>
{
    public FluentObject(TObject o)
    {
        Object = o;
        CurrentStateId = new Initiable<TStates>();
    }

    public Initiable<TStates> CurrentStateId { get; set; }
    public TObject Object { get; set; }
}

public class Initiable<T>
{
    private T _data;
    private bool _isInit;

    public T Data
    {
        get => _data;
        set
        {
            _data = value;
            _isInit = true;
        }
    }

    public bool IsInit => _isInit;
}