using System.Text.RegularExpressions;
using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Abstract.Messages;
using Bot.Commands.Core.Fluent;
using Bot.Commands.Core.Fluent.Builders.CallbackBuilders.Extensions;
using Bot.Commands.Core.Fluent.Builders.Extensions;
using Bot.Commands.Core.Fluent.Builders.StateMachineBuilders;
using Bot.Commands.Core.Fluent.StateMachine;
using Bot.Commands.Core.Models;

namespace SurveyComparison;

public abstract class SurveyFlow<TMessage>(Output output) : FluentCommand<SurveyData, SurveyStep> where TMessage : class
{
    protected override Task<(SurveyData, SurveyStep)> Entry<T>(IQueryContext<T> query, SurveyData data)
        => Task.FromResult((new SurveyData(), SurveyStep.Name));

    protected override IStateMachine<SurveyStep> StateMachine(IStateMachineBuilder<SurveyData, SurveyStep> b)
        => b.State(SurveyStep.Name)
            .WithMessage((s, d) => Prompt(d, "Имя (2–50 символов):"), Send)
            .WithCallbacks().Row().NextFromCallback("Отмена", "", SurveyStep.Cancelled, true)
            .Next<TMessage>(ReadName, true)
          .State(SurveyStep.Age)
            .WithMessage((s, d) => Prompt(d, "Возраст (18–120):"), Send)
            .WithCallbacks()
                .Row().NextFromCallback("Назад", "", SurveyStep.Name, true)
                .Row().NextFromCallback("Отмена", "", SurveyStep.Cancelled, true)
            .Next<TMessage>(ReadAge, true)
          .State(SurveyStep.Confirm)
            .WithMessage((s, d) => Prompt(d, $"Проверьте: {d.Name}, {d.Age}"), Send)
            .WithCallbacks()
                .Row().NextFromCallback("Назад", "", SurveyStep.Age, true)
                .Row().NextFromCallback("Отмена", "", SurveyStep.Cancelled, true)
                .Row().NextFromCallback("Подтвердить", "", SurveyStep.Done, true)
            .Loop(true)
          .Exit<object>(SurveyStep.Done, (q, s, d) => Finish($"Готово: {d.Name}, {d.Age}"))
          .Exit<object>(SurveyStep.Cancelled, (q, s, d) => Finish("Анкета отменена."))
          .Build();

    private static Task<SurveyStep> ReadName(IQueryContext<TMessage> q, SurveyStep s, SurveyData d)
    {
        var name = q.GetData()?.Trim() ?? "";
        if (!Regex.IsMatch(name, @"^[A-Za-zА-Яа-яЁё][A-Za-zА-Яа-яЁё '-]{1,49}$"))
        {
            d.Error = "Некорректное имя.";
            return Task.FromResult(SurveyStep.Name);
        }
        d.Name = name;
        d.Error = null;
        return Task.FromResult(SurveyStep.Age);
    }
    private static Task<SurveyStep> ReadAge(IQueryContext<TMessage> q, SurveyStep s, SurveyData d)
    {
        var text = q.GetData()?.Trim() ?? "";
        if (!Regex.IsMatch(text, "^[0-9]{1,3}$") || !int.TryParse(text, out var age) || age < 18 || age > 120)
        {
            d.Error = "Некорректный возраст.";
            return Task.FromResult(SurveyStep.Age);
        }
        d.Age = age;
        d.Error = null;
        return Task.FromResult(SurveyStep.Confirm);
    }
    private static Task<string> Prompt(SurveyData data, string prompt)
    {
        var text = data.Error == null ? prompt : data.Error + " " + prompt;
        data.Error = null;
        return Task.FromResult(text);
    }
    private Task Send(IQueryContext<object> q, SurveyStep s, SurveyData d, IBotMessage message)
    {
        output.Show(message);
        return Task.CompletedTask;
    }
    private Task<ICommandExecutionResult> Finish(string text)
    {
        output.Show(new BotMessage(text, null, null, null));
        return Task.FromResult<ICommandExecutionResult>(CommandExecutionResult.Break());
    }
}
