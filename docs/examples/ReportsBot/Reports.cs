using System.Globalization;
using System.Net;
using System.Text;
using Bot.Commands.UI.DropDown;
using Bot.Commands.UI.Pagination;

namespace ReportsBot;

public enum ReportStep { Period, Departments, Format, Confirm, Done, Cancelled }

public sealed class ReportData
{
    public Guid RequestId { get; set; } = Guid.NewGuid();
    public string Period { get; set; }
    public string Format { get; set; } = "csv";
    public string Error { get; set; }
    public DropDown<string> Departments { get; set; } = new()
    {
        Title = "Подразделения", CallbackData = "departments", Show = true,
        Mode = DropDownMode.Multiple,
        Items = new[] { ("sales", "Продажи"), ("production", "Производство"), ("logistics", "Логистика") }
            .Select(x => new DropDownMenuItem<string> { Item = x.Item1, Title = x.Item2, CallbackData = x.Item1 }).ToArray()
    };
    public string Summary => $"Период: {Period}\nПодразделения: {string.Join(", ", Departments.GetSelected().Select(x => x.Title))}\nФормат: {Format.ToUpperInvariant()}";
}

public sealed class ArchiveData : IFluentPagination, IFluentPaginationMenu
{
    public long ChatId { get; set; }
    public long UserId { get; set; }
    public uint PageNumber { get; set; } = 1;
    public ulong TotalCount { get; set; }
    public uint Limit => 3;
    // Stable snapshot: a callback always refers to the list that was shown.
    public List<ReportFile> Reports { get; set; } = new();
}

public sealed record ReportFile(Guid Id, long ChatId, long UserId, string Period, string Format, string Path);

// Sequential demo repository. Files persist; the archive index and sessions do not.
public sealed class ReportRepository(string directory)
{
    private readonly List<ReportFile> _reports = new();
    public IReadOnlyList<ReportFile> For(long chat, long user) => _reports.Where(x => x.ChatId == chat && x.UserId == user).Reverse().ToArray();

    public async Task<ReportFile> Create(long chat, long user, ReportData data)
    {
        var existing = _reports.SingleOrDefault(x => x.Id == data.RequestId && x.ChatId == chat && x.UserId == user);
        if (existing != null) return existing;
        if (!DateTime.TryParseExact(data.Period, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var period)
            || data.Departments.GetSelected().Length == 0 || data.Format is not ("csv" or "html"))
            throw new InvalidOperationException("Incomplete report parameters.");

        // Deterministic fictional monthly totals, not production/business data.
        var rows = data.Departments.GetSelected().Select(x =>
            (Name: x.Title, Count: period.Month * 10 + Array.IndexOf(data.Departments.Items, x) + 1)).ToArray();
        string content;
        if (data.Format == "csv")
            content = "Period,Department,Operations\r\n" + string.Join("\r\n", rows.Select(x => $"{data.Period},{x.Name},{x.Count}")) + "\r\n";
        else
            content = "<!doctype html><html lang=\"ru\"><meta charset=\"utf-8\"><title>Отчёт</title>"
                + "<style>body{font:16px system-ui;max-width:800px;margin:40px auto}table{border-collapse:collapse;width:100%}td,th{padding:12px;border:1px solid #ccc;text-align:left}</style>"
                + $"<h1>Отчёт за {WebUtility.HtmlEncode(data.Period)}</h1><p>Демонстрационные данные</p><table><tr><th>Подразделение</th><th>Операций</th></tr>"
                + string.Join("", rows.Select(x => $"<tr><td>{WebUtility.HtmlEncode(x.Name)}</td><td>{x.Count}</td></tr>")) + "</table></html>";
        Directory.CreateDirectory(directory);
        var path = System.IO.Path.GetFullPath(System.IO.Path.Combine(directory, $"report-{data.Period}-{data.RequestId:N}.{data.Format}"));
        await File.WriteAllTextAsync(path, content, new UTF8Encoding(true));
        var report = new ReportFile(data.RequestId, chat, user, data.Period, data.Format, path);
        _reports.Add(report);
        return report;
    }
}
