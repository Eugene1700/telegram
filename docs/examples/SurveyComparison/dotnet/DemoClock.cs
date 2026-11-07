using Bot.Commands.Abstract.Interfaces;
namespace SurveyComparison;
public sealed class DemoClock : IClock { public DateTime Now => DateTime.UtcNow; }
