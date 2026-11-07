namespace Bot.Commands.Core.Fluent
{

    public class Initiable<T>
    {
        public T Data { get; set; }

        public bool IsInit { get; set; }
    }
}
