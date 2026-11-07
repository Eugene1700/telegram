using Bot.Commands.Abstract.Messages;
namespace Bot.Commands.Telegram;
/// <summary>Identifies text formatted for Telegram.</summary>
public sealed record TelegramTextFormat(global::Telegram.Bot.Types.Enums.ParseMode Mode) : IMessageTextFormat;
