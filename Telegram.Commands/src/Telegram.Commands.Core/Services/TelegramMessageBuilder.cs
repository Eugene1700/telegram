using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Telegram.Commands.Abstract.Messages;

namespace Telegram.Commands.Core.Services
{

    public abstract class MessageBuilderBase: IInlineMarkupQueryBuilder
    {
        private readonly List<(string, EndItemType, bool)> _items;
        private byte[] _photo;
        private readonly InlineMarkupQueryBuilder _inlineMarkupBuilder;

        protected abstract BotMessageParseMode ParseMode { get; }
        
        protected MessageBuilderBase()
        {
            _items = new List<(string, EndItemType, bool)>();
            _inlineMarkupBuilder = new InlineMarkupQueryBuilder();
        }
        
        public MessageBuilderBase AppendPlain(string text)
        {
            Append(text, false);
            return this;
        }
        
        protected void Append(string text, bool isMarking)
        {
            _items.Add((text, EndItemType.AppEnd, isMarking));
        }
        
        public MessageBuilderBase AddPhoto(byte[] photo)
        {
            _photo = photo;
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
            AppendSpace(text, false);
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
        
        public IBotMessage[] GetMessages(int? messageLenghtConstraint = 4096)
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
                    res.Add(new BotMessage(sb.ToString(), _photo, ParseMode, new ReadOnlyCollection<CallbackDataWithCommand>(Array.Empty<CallbackDataWithCommand>())));
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
            res.Add(new BotMessage(sb.ToString(), _photo, ParseMode, _inlineMarkupBuilder.GetCallbacks()));

            return res.ToArray();
        }

        protected abstract string PrepareText(string text);
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
    
    
    public class MarkupMessageBuilder: MessageBuilderBase
    {
        protected override BotMessageParseMode ParseMode => BotMessageParseMode.MarkDown;
        protected override string PrepareText(string text)
        {
            return text.ReplaceMarkDownEscapeSymbols();
        }

        public MarkupMessageBuilder AppendMarkdown(string text)
        {
            Append(text, true);
            return this;
        }

        public MarkupMessageBuilder AppendCopyContent(string plainContent)
        {
            AppendMarkdown($"`");
            AppendPlain(plainContent);
            AppendMarkdown("`");

            return this;
        }

        public MarkupMessageBuilder AppendCopyContentLine(string plainContent)
        {
            AppendCopyContent(plainContent);
            AppendEmptyLine();
            return this;
        }

        public MarkupMessageBuilder AppendBold(string plainContent)
        {
            AppendMarkdown($"*");
            AppendPlain(plainContent);
            AppendMarkdown("*");
            return this;
        }
        public MarkupMessageBuilder AppendBoldLine(string plainContent)
        {
            AppendBold(plainContent);
            AppendEmptyLine();
            return this;
        }

        public MarkupMessageBuilder AppendUnderline(string plainContent)
        {
            AppendMarkdown("__");
            AppendPlain(plainContent);
            AppendMarkdown("__");
            return this;
        }
        public MarkupMessageBuilder AppendUnderlineLine(string plainContent)
        {
            AppendUnderline(plainContent);
            AppendEmptyLine();
            return this;
        }
        
        public MarkupMessageBuilder AppendLinkLine(string link, string title)
        {
            AppendLink(link, title);
            AppendEmptyLine();
            return this;
        }
        
        public MarkupMessageBuilder AppendLink(string link, string title)
        {
            AppendMarkdown($"[");
            AppendPlain(title);
            AppendMarkdown($"]({link})");
            return this;
        }
    }

    internal enum EndItemType
    {
        NewLine,
        Space,
        AppEnd
    }
}