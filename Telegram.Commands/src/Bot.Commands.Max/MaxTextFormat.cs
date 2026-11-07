using Bot.Commands.Abstract.Messages;
namespace Bot.Commands.Max;
/// <summary>Identifies text formatted for Max.</summary>
public sealed record MaxTextFormat(global::Max.Bot.Types.Enums.TextFormat Mode) : IMessageTextFormat;
