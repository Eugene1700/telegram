namespace Telegram.Commands.Core.Fluent;

public class FluentObject<TObject, TStates>
{
    public FluentObject(TObject o, FireType? fireType)
    {
        Object = o;
        CurrentStateId = new Initiable<TStates>();
        FireType = fireType;
    }

    public Initiable<TStates> CurrentStateId { get; set; }
    public TObject Object { get; set; }
    public FireType? FireType { get; set; }
}

public enum FireType
{
    Entry,
    HandleCurrent
}

public class Initiable<T>
{
    public T Data { get; set; }

    public bool IsInit { get; set; }
}