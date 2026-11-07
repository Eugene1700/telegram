namespace Bot.Commands.Core.Fluent
{
    public class FluentObject<TObject, TStates>
    {
        public FluentObject() : this(default, null)
        {
        }

        public FluentObject(TObject o, FireType? fireType)
        {
            Object = o;
            CurrentStateId = new Initiable<TStates>();
            FireType = fireType;
        }

        public Initiable<TStates> CurrentStateId { get; set; }
        public TObject Object { get; set; }
        public FireType? FireType { get; set; }
        public Initiable<TStates> ParentStateId { get; set; }
    }
}
