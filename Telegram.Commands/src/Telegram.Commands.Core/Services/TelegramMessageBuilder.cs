using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Commands.Abstract.Messages;
using Telegram.Commands.Core.Fluent;
using Telegram.Commands.Core.Messages;

namespace Telegram.Commands.Core.Services
{
    public class TelegramMessageBuilder
    {
        private List<(string, TelegramParseMode, EndItemType)> _items;
        private byte[] _photo;
        private IReplyMarkup _replyMarkUp;

        public TelegramMessageBuilder()
        {
            _items = new List<(string, TelegramParseMode, EndItemType)>();
        }

        public TelegramMessageBuilder Append(string text, TelegramParseMode telegramParseMode)
        {
            _items.Add((text, telegramParseMode, EndItemType.AppEnd));
            return this;
        }
        
        public TelegramMessageBuilder Append((string, TelegramParseMode) text)
        {
            return Append(text.Item1, text.Item2);
        }

        public TelegramMessageBuilder AddPhoto(byte[] photo)
        {
            _photo = photo;
            return this;
        }
        
        public TelegramMessageBuilder AddReplyMarkUp(IReplyMarkup replyMarkup)
        {
            _replyMarkUp = replyMarkup;
            return this;
        }
        
        public TelegramMessageBuilder AppendLine(string text, TelegramParseMode telegramParseMode)
        {
            _items.Add((text, telegramParseMode, EndItemType.NewLine));
            return this;
        }
        
        public TelegramMessageBuilder AppendLine((string, TelegramParseMode) message)
        {
            _items.Add((message.Item1, message.Item2, EndItemType.NewLine));
            return this;
        }
        
        public TelegramMessageBuilder AppendSpace(string text, TelegramParseMode telegramParseMode)
        {
            _items.Add((text, telegramParseMode, EndItemType.Space));
            return this;
        }
        
        public TelegramMessageBuilder AppendSpacePlain(string text)
        {
            return AppendSpace(text, TelegramParseMode.Plain);
        }

        public (string, TelegramParseMode) GetTextMessage()
        {
            return GetTextMessages(null).First();
        }
        
        public (string, TelegramParseMode)[] GetTextMessages(int? messageLenghtConstraint = 4096)
        {
            var parseMode = DetectParseMode();

            var res = new List<(string, TelegramParseMode)>();
            var sb = new StringBuilder();
            foreach (var (text, itemParseMode, endItemType) in _items)
            {
                var textNew = text;
                if (itemParseMode == TelegramParseMode.Plain && 
                    parseMode == TelegramParseMode.MarkDownV2)
                {
                    textNew = text.ReplaceMarkDownEscapeSymbols(parseMode);
                }

                if (messageLenghtConstraint.HasValue && sb.Length + textNew.Length > messageLenghtConstraint.Value)
                {
                    res.Add((sb.ToString(), parseMode));
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
                        sb.Append($"{textNew} ");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            res.Add((sb.ToString(), parseMode));

            return res.ToArray();
        }

        private TelegramParseMode DetectParseMode()
        {
            if (_items.Any(x => x.Item2 == TelegramParseMode.MarkDownV2))
            {
                return TelegramParseMode.MarkDownV2;
            }
            
            if (_items.Any(x => x.Item2 == TelegramParseMode.MarkDown))
            {
                return TelegramParseMode.MarkDown;
            }
            
            if (_items.Any(x => x.Item2 == TelegramParseMode.Html))
            {
                return TelegramParseMode.Html;
            }

            return TelegramParseMode.Plain;
        }

        public TelegramMessageBuilder AppendPlain(string text)
        {
            return Append(text, TelegramParseMode.Plain);
        }

        public TelegramMessageBuilder AppendLinePlain(string s)
        {
            return AppendLine(s, TelegramParseMode.Plain);
        }

        public TelegramMessageTyped<TelegramParseMode> GetTelegramMessage()
        {
            var (text, parseMode) = GetTextMessage();
            return new TelegramMessageTyped<TelegramParseMode>
            {
                Text = text,
                ParseMode = parseMode,
                ReplyMarkup = _replyMarkUp,
                Photo = _photo
            };
        }

        public TelegramMessageBuilder AppendEmptyLine()
        {
            return AppendLinePlain(string.Empty);
        }
        
        public TelegramMessageBuilder AppendEmpty()
        {
            return AppendPlain(string.Empty);
        }

        public TelegramMessageBuilder AppendMarkdownV2(string s)
        {
            return Append(s, TelegramParseMode.MarkDownV2);
        }

        public TelegramMessageBuilder AppendPlainIfMessageExist(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                AppendEmpty();
            }
            else
            {
                AppendPlain(message);
            }

            return this;
        }

        public TelegramMessageBuilder AppendMarkdownV2CopyContent(string plainContent)
        {
            AppendMarkdownV2($"`");
            AppendPlain(plainContent);
            AppendMarkdownV2("`");

            return this;
        }

        public TelegramMessageBuilder AppendMarkdownV2CopyContentLine(string plainContent)
        {
            AppendMarkdownV2CopyContent(plainContent);
            AppendEmptyLine();
            return this;
        }

        public TelegramMessageBuilder AppendBold(string plainContent)
        {
            AppendMarkdownV2($"*");
            AppendPlain(plainContent);
            AppendMarkdownV2("*");
            return this;
        }
        public TelegramMessageBuilder AppendBoldLine(string plainContent)
        {
            AppendBold(plainContent);
            AppendEmptyLine();
            return this;
        }

        public TelegramMessageBuilder AppendUnderline(string plainContent)
        {
            AppendMarkdownV2("__");
            AppendPlain(plainContent);
            AppendMarkdownV2("__");
            return this;
        }
        public TelegramMessageBuilder AppendUnderlineLine(string plainContent)
        {
            AppendUnderline(plainContent);
            AppendEmptyLine();
            return this;
        }
        
        public TelegramMessageBuilder AppendLinkLine(string link, string title)
        {
            AppendLink(link, title);
            AppendEmptyLine();
            return this;
        }
        
        public TelegramMessageBuilder AppendLink(string link, string title)
        {
            AppendMarkdownV2($"[");
            AppendPlain(title);
            AppendMarkdownV2($"]({link})");
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