namespace Bot.Commands.Telegram.Extensions
{
    public static class TelegramFormatsHelper
    {
        public static string ReplaceMarkDownEscapeSymbols(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;
            var res = input.Replace(@"\", @"\\").Replace("_", @"\_");
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
