namespace Telegram.Commands.Core.Fluent
{
    public class FluentObject<TProps, TStates>
    {
        public FluentObject(TProps o, FireType? fireType)
        {
            Object = o;
            CurrentStateId = new Initiable<TStates>();
            FireType = fireType;
        }

        public Initiable<TStates> CurrentStateId { get; set; }
        public TProps Object { get; set; }
        public FireType? FireType { get; set; }
        public Initiable<TStates> ParentStateId { get; set; }
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
}