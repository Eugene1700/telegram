using Bot.Commands.Abstract.Interfaces;
namespace SurveyComparison;
public sealed class DemoProfile : IBotProfile
{
    public string BotName => "survey";
    public string Key => "";
    public string BaseUrl => "";
    public string UpdateRoute => "";
    public string[] Swarms => null;
    public GroupMessageHandlingStrategy GroupMessageHandlingStrategy => GroupMessageHandlingStrategy.Direct;
}
