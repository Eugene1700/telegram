namespace Bot.Commands.Abstract
{
    public class BotResult
    {
        public bool Ok { get; }

        public BotResult(bool ok)
        {
            Ok = ok;
        }
    }
}