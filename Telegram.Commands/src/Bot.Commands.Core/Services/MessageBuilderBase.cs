using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Bot.Commands.Abstract.Messages;

namespace Bot.Commands.Core.Services
{

    public abstract class MessageBuilderBase: IInlineMarkupQueryBuilder
    {
        private readonly List<(string, EndItemType, bool)> _items;
        private byte[] _photo;
        private bool _sendAsFile;
        private string _fileName;
        private readonly InlineMarkupQueryBuilder _inlineMarkupBuilder;

        protected abstract IMessageTextFormat Format { get; }
        
        protected MessageBuilderBase()
        {
            _items = new List<(string, EndItemType, bool)>();
            _inlineMarkupBuilder = new InlineMarkupQueryBuilder();
        }
        
        public MessageBuilderBase AppendPlain(string text)
        {
            Append(text, true);
            return this;
        }
        
        protected void Append(string text, bool isMarking)
        {
            _items.Add((text, EndItemType.AppEnd, isMarking));
        }
        
        public MessageBuilderBase AddPhoto(byte[] photo)
        {
            _photo = photo;
            _sendAsFile = false;
            _fileName = null;
            return this;
        }

        public MessageBuilderBase AddFile(byte[] content, string fileName)
        {
            if (content == null || content.Length == 0) throw new ArgumentException("File content is required.", nameof(content));
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("File name is required.", nameof(fileName));
            _photo = content;
            _sendAsFile = true;
            _fileName = fileName;
            return this;
        }
        
        public MessageBuilderBase AppendPlainLine(string text)
        {
            AppendLine(text, true);
            return this;
        }
        
        protected void AppendLine(string text, bool isMarking)
        {
            _items.Add((text, EndItemType.NewLine, isMarking));
        }
        
        protected MessageBuilderBase AppendSpace(string text, bool isMarking)
        {
            _items.Add((text, EndItemType.Space, isMarking));
            return this;
        }
        
        public MessageBuilderBase AppendPlainSpace(string text)
        {
            AppendSpace(text, true);
            return this;
        }
        
        public MessageBuilderBase AppendEmptyLine()
        {
            return AppendPlainLine(string.Empty);
        }
        
        public MessageBuilderBase AppendEmpty()
        {
            return AppendPlain(string.Empty);
        }
        
        public IBotMessage[] GetMessages(int? messageLenghtConstraint = null)
        {
            var res = new List<IBotMessage>();
            var sb = new StringBuilder();
            foreach (var (text, endItemType, isMarking) in _items)
            {
                var textNew = text;
                
                if (isMarking)
                {
                    textNew = PrepareText(text);
                }

                if (messageLenghtConstraint.HasValue && sb.Length + textNew.Length > messageLenghtConstraint.Value)
                {
                    res.Add(new BotMessage(sb.ToString(), _photo, Format, new ReadOnlyCollection<CallbackDataWithCommand>(Array.Empty<CallbackDataWithCommand>())) { SendAsFile = _sendAsFile, FileName = _fileName });
                    sb.Clear();
                }
                switch (endItemType)
                {
                    case EndItemType.NewLine:
                        sb.AppendLine($"{textNew}");
                        break;
                    case EndItemType.Space:
                        sb.Append($"{textNew} ");
                        break;
                    case EndItemType.AppEnd:
                        sb.Append($"{textNew}");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            res.Add(new BotMessage(sb.ToString(), _photo, Format, _inlineMarkupBuilder.GetCallbacks()) { SendAsFile = _sendAsFile, FileName = _fileName });

            return res.ToArray();
        }

        protected abstract string PrepareText(string text);
        public IInlineMarkupQueryBuilder AddInlineKeyboardButton<TCommand, TQuery>(CallbackData callbackData)
            where TCommand : Bot.Commands.Abstract.Commands.IQueryCommand<TQuery>
        {
            _inlineMarkupBuilder.AddInlineKeyboardButton<TCommand, TQuery>(callbackData);
            return this;
        }

        public IInlineMarkupQueryBuilder AddInlineKeyboardButton<TCommand, TSessionObject, TQuery>(CallbackData callbackData)
            where TCommand : Bot.Commands.Abstract.Commands.ISessionCommand<TQuery, TSessionObject>
        {
            _inlineMarkupBuilder.AddInlineKeyboardButton<TCommand, TSessionObject, TQuery>(callbackData);
            return this;
        }

        public IInlineMarkupQueryBuilder InlineKeyboardButtonsRow(CallbackDataWithCommand[] rows)
        {
            _inlineMarkupBuilder.InlineKeyboardButtonsRow(rows);
            return this;
        }

        public ReadOnlyCollection<CallbackDataWithCommand> GetCallbacks()
        {
            return _inlineMarkupBuilder.GetCallbacks();
        }
    }
}
