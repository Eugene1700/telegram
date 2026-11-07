using Bot.Commands.Abstract.Attributes;
using Telegram.Bot.Types;
namespace SurveyComparison;

[Command(Name = "survey", Authorized = false)]
public sealed class SurveyCommand(Output output) : SurveyFlow<Message>(output) { }
