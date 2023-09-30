using Telegram.Commands.Core.Fluent;

namespace Telegram.Commands.Core.Services
{
    public static class TelegramFormatsHelper
    {
        public static string ReplaceMarkDownEscapeSymbols(this string input, TelegramParseMode telegramParseMode)
        {
            if (!telegramParseMode.IsMarkdown())
            {
                return input;
            }
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;
            var res = input.Replace("_", @"\_");
            res = res.Replace("*", @"\*");
            res = res.Replace("(", @"\(");
            res = res.Replace(")", @"\)");
            res = res.Replace("[", @"\[");
            res = res.Replace("]", @"\]");
            res = res.Replace("~", @"\~");
            res = res.Replace("`", @"\`");
            res = res.Replace("|", @"\|");
            res = res.Replace(".", @"\.");
            res = res.Replace("-", @"\-");
            res = res.Replace(">", @"\>");
            res = res.Replace("=", @"\=");
            res = res.Replace("+", @"\+");
            res = res.Replace("{", @"\{");
            res = res.Replace("}", @"\}");
            res = res.Replace("!", @"\!");
            res = res.Replace("#", @"\#");
            return res;
        }
    }
}