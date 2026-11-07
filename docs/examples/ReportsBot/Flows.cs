using System.Globalization;
using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Abstract.Messages;
using Bot.Commands.Core.Fluent;
using Bot.Commands.Core.Fluent.Builders.CallbackBuilders;
using Bot.Commands.Core.Fluent.Builders.CallbackBuilders.Extensions;
using Bot.Commands.Core.Fluent.Builders.Extensions;
using Bot.Commands.Core.Fluent.Builders.StateMachineBuilders;
using Bot.Commands.Core.Fluent.StateMachine;
using Bot.Commands.Core.Models;
using Bot.Commands.UI.DropDown;
using Bot.Commands.UI.Pagination;
using Tg = Telegram.Bot.Types;
using Mx = Max.Bot.Types;

namespace ReportsBot;

public abstract class DemoFlow<TData, TState>(IOutput output) : FluentCommand<TData, TState>
{
    protected IOutput Output { get; } = output;
    protected Task Send(IQueryContext<object> query, TState state, TData data, IBotMessage message)
        => Output.Show(query.GetChatId(), message, query.GetMessageId(), query.Query is Tg.CallbackQuery or Mx.CallbackQuery);

    protected override InterceptResult<TQuery, TData> TryQueryCommandIntercept<TQuery>(TState state,
        IQueryCommand<TQuery> command, IQueryContext<TQuery> query, TData data)
        => new() { MustIntercept = command is HelpCommand or CancelCommand, IsTerminalResult = command is CancelCommand };
}

[Command(Name = "report", Authorized = false)]
public abstract class ReportFlow<TMessage, TCallback>(IOutput output, ReportRepository reports)
    : DemoFlow<ReportData, ReportStep>(output) where TMessage : class where TCallback : class
{
    protected override Task<(ReportData, ReportStep)> Entry<TQuery>(IQueryContext<TQuery> query, ReportData data)
        => Task.FromResult((data ?? new ReportData(), ReportStep.Period));

    protected override IStateMachine<ReportStep> StateMachine(IStateMachineBuilder<ReportData, ReportStep> builder)
        => builder
            .State(ReportStep.Period)
            .WithMessage((s, d) => Task.FromResult($"{d.Error}\nВведите период в формате ГГГГ-ММ, например 2026-10.".Trim()), Send)
            .Next<TMessage>(ReadPeriod, true)
            .State(ReportStep.Departments)
            .WithMessage((s, d) => Task.FromResult($"{d.Error}\nВыберите хотя бы одно подразделение.".Trim()), Send)
            .WithCallbacks().KeyBoard(Departments)
            .Next<TMessage>((q, s, d) => Task.FromResult(s), true)
            .State(ReportStep.Format)
            .WithMessage("Выберите формат. HTML можно распечатать в PDF через браузер.", Send)
            .WithCallbacks()
            .Row().OnCallback<ReportData, ReportStep, TCallback>("CSV", "csv", ChooseFormat, true)
            .OnCallback<ReportData, ReportStep, TCallback>("HTML", "html", ChooseFormat, true)
            .Row().NextFromCallback("Назад к подразделениям", "back", ReportStep.Departments, true)
            .Next<TMessage>((q, s, d) => Task.FromResult(s), true)
            .State(ReportStep.Confirm)
            .WithMessage((s, d) => Task.FromResult(d.Summary + "\nСформировать отчёт?"), Send)
            .WithCallbacks()
            .Row().NextFromCallback("Сформировать", "create", ReportStep.Done, true)
            .Row().NextFromCallback("Изменить период", "period", ReportStep.Period, true)
            .NextFromCallback("Изменить подразделения", "departments", ReportStep.Departments, true)
            .Row().NextFromCallback("Изменить формат", "format", ReportStep.Format, true)
            .NextFromCallback("Отмена", "cancel", ReportStep.Cancelled, true)
            .Next<TMessage>((q, s, d) => Task.FromResult(s), true)
            .Exit<object>(ReportStep.Done, Finish)
            .Exit<object>(ReportStep.Cancelled, Cancel)
            .Build();

    private Task<ReportStep> ReadPeriod(IQueryContext<TMessage> query, ReportStep state, ReportData data)
    {
        if (!DateTime.TryParseExact(query.GetData()?.Trim(), "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var period))
        {
            data.Error = "Некорректный период.";
            return Task.FromResult(ReportStep.Period);
        }
        data.Error = null;
        data.Period = period.ToString("yyyy-MM", CultureInfo.InvariantCulture);
        return Task.FromResult(ReportStep.Departments);
    }

    private Task Departments(ReportStep state, ReportData data, ICallbacksBuilderBase<ReportData, ReportStep> builder)
    {
        builder.AddDropDown<string, ReportData, ReportStep, TCallback>(data.Departments,
            (q, s, d, value) => { d.Error = null; return Task.FromResult(s); });
        builder.Row().OnCallback<ReportData, ReportStep, TCallback>("Продолжить", "next", (q, s, d, value) =>
        {
            d.Error = d.Departments.GetSelected().Length == 0 ? "Выберите подразделение перед продолжением." : null;
            return Task.FromResult(d.Error == null ? ReportStep.Format : ReportStep.Departments);
        }, true);
        builder.Row().NextFromCallback("Назад к периоду", "back", ReportStep.Period, true);
        return Task.CompletedTask;
    }

    private Task<ReportStep> ChooseFormat(IQueryContext<TCallback> query, ReportStep state, ReportData data, string format)
    {
        data.Format = format;
        return Task.FromResult(ReportStep.Confirm);
    }

    private async Task<ICommandExecutionResult> Finish(IQueryContext<object> query, ReportStep state, ReportData data)
    {
        var report = await reports.Create(query.GetChatId(), query.GetFromId(), data);
        await Output.File(query.GetChatId(), report);
        await Output.Text(query.GetChatId(), "Отчёт готов. /archive — архив, /report — новый отчёт.");
        return CommandExecutionResult.Break();
    }

    private async Task<ICommandExecutionResult> Cancel(IQueryContext<object> query, ReportStep state, ReportData data)
    {
        await Output.Text(query.GetChatId(), "Создание отчёта отменено.");
        return CommandExecutionResult.Break();
    }
}

[Command(Name = "report", Authorized = false, Swarms = new[] { "telegram" })]
public sealed class TelegramReport(IOutput output, ReportRepository reports) : ReportFlow<Tg.Message, Tg.CallbackQuery>(output, reports);
[Command(Name = "report", Authorized = false, Swarms = new[] { "max" })]
public sealed class MaxReport(IOutput output, ReportRepository reports) : ReportFlow<Mx.Message, Mx.CallbackQuery>(output, reports);

public enum ArchiveStep { Browse, Done }

[Command(Name = "archive", Authorized = false)]
public abstract class ArchiveFlow<TMessage, TCallback>(IOutput output, ReportRepository reports)
    : DemoFlow<ArchiveData, ArchiveStep>(output) where TMessage : class where TCallback : class
{
    protected override Task<(ArchiveData, ArchiveStep)> Entry<TQuery>(IQueryContext<TQuery> query, ArchiveData data)
        => Task.FromResult((new ArchiveData { ChatId = query.GetChatId(), UserId = query.GetFromId(), Reports = reports.For(query.GetChatId(), query.GetFromId()).ToList() }, ArchiveStep.Browse));

    protected override IStateMachine<ArchiveStep> StateMachine(IStateMachineBuilder<ArchiveData, ArchiveStep> builder)
        => builder.State(ArchiveStep.Browse)
            .WithMessage((s, d) => Task.FromResult(d.Reports.Count == 0 ? "Архив пуст. Закройте его и создайте отчёт через /report." : $"Архив: {d.Reports.Count} отчётов. Страница {d.PageNumber}."), Send)
            .WithCallbacks().KeyboardWithPagination<ArchiveData, ArchiveStep, TCallback>(Page)
            .Row().NextFromCallback("Закрыть архив", "close", ArchiveStep.Done, true)
            .Next<TMessage>((q, s, d) => Task.FromResult(s), true)
            .Exit<object>(ArchiveStep.Done, Finish).Build();

    private Task<IFluentPaginationMenu> Page(ArchiveData data, uint page, ICallbacksBuilderBase<ArchiveData, ArchiveStep> builder)
    {
        data.TotalCount = (ulong)data.Reports.Count;
        foreach (var report in data.Reports.Skip((int)((page - 1) * data.Limit)).Take((int)data.Limit))
            builder.Row().OnCallback<ArchiveData, ArchiveStep, TCallback>($"{report.Period} · {report.Format.ToUpperInvariant()} · {report.Id.ToString("N")[..6]}", report.Id.ToString("N"), async (q, s, d, id) =>
            {
                var file = d.Reports.Single(x => x.Id.ToString("N") == id && x.ChatId == q.GetChatId() && x.UserId == q.GetFromId());
                await Output.File(q.GetChatId(), file);
                return s;
            }, false);
        return Task.FromResult<IFluentPaginationMenu>(data);
    }

    private async Task<ICommandExecutionResult> Finish(IQueryContext<object> q, ArchiveStep s, ArchiveData d)
    {
        await Output.Text(q.GetChatId(), "Архив закрыт. /report — новый отчёт.");
        return CommandExecutionResult.Break();
    }
}

[Command(Name = "archive", Authorized = false, Swarms = new[] { "telegram" })]
public sealed class TelegramArchive(IOutput output, ReportRepository reports) : ArchiveFlow<Tg.Message, Tg.CallbackQuery>(output, reports);
[Command(Name = "archive", Authorized = false, Swarms = new[] { "max" })]
public sealed class MaxArchive(IOutput output, ReportRepository reports) : ArchiveFlow<Mx.Message, Mx.CallbackQuery>(output, reports);

[Command(Name = "help", Authorized = false)]
public class HelpCommand(IOutput output) : IQueryCommand<Tg.Message>, IQueryCommand<Mx.Message>
{
    public Task<ICommandExecutionResult> Execute(IQueryContext<Tg.Message> q) => Reply(q.GetChatId());
    public Task<ICommandExecutionResult> Execute(IQueryContext<Mx.Message> q) => Reply(q.GetChatId());
    private async Task<ICommandExecutionResult> Reply(long chat)
    {
        await output.Text(chat, "/report — создать отчёт\n/archive — архив\n/cancel — отменить диалог\n/help — справка без потери шага\nДанные демонстрационные; архив и сессии хранятся до перезапуска.");
        return CommandExecutionResult.Freeze();
    }
}

[Command(Name = "start", Authorized = false)]
public sealed class StartCommand(IOutput output) : HelpCommand(output);

[Command(Name = "cancel", Authorized = false)]
public sealed class CancelCommand(IOutput output) : IQueryCommand<Tg.Message>, IQueryCommand<Mx.Message>
{
    public Task<ICommandExecutionResult> Execute(IQueryContext<Tg.Message> q) => Reply(q.GetChatId());
    public Task<ICommandExecutionResult> Execute(IQueryContext<Mx.Message> q) => Reply(q.GetChatId());
    private async Task<ICommandExecutionResult> Reply(long chat)
    {
        await output.Text(chat, "Диалог отменён. /report — начать заново.");
        return CommandExecutionResult.Break();
    }
}

[Event(Type = EventType.ChatMemberAdded, Authorized = false, Area = ChatArea.Group | ChatArea.SuperGroup)]
public sealed class WelcomeCommand(IOutput output) : IQueryCommand<Tg.Message>, IQueryCommand<Mx.UserAddedUpdate>
{
    public Task<ICommandExecutionResult> Execute(IQueryContext<Tg.Message> q) => Reply(q.GetChatId());
    public Task<ICommandExecutionResult> Execute(IQueryContext<Mx.UserAddedUpdate> q) => Reply(q.GetChatId());
    private async Task<ICommandExecutionResult> Reply(long chat)
    {
        await output.Text(chat, "Добро пожаловать! Откройте личный чат с ботом и отправьте /report для подготовки отчёта.");
        return CommandExecutionResult.Freeze();
    }
}
