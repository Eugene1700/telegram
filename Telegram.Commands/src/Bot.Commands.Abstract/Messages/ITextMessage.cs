using System.Collections.ObjectModel;

namespace Bot.Commands.Abstract.Messages
{
    public interface ITextMessage
    {
        string Text { get; }
        public byte[] Photo { get; }
        /// <summary>Send the attachment as a file instead of displaying it as a photo.</summary>
        bool SendAsFile => false;
        /// <summary>Optional attachment filename, including its extension.</summary>
        string FileName => null;
        public IMessageTextFormat Format { get;}
    }
}
