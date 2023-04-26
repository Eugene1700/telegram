using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Commands.Core.Fluent.Builders.CallbackBuilders;
using Telegram.Commands.Core.Fluent.Builders.CallbackBuilders.Extensions;

namespace Telegram.Commands.UI.Pagination;

public static class FluentCommandPaginationMenu
{
    public static ICallbacksBuilder<TObj, TStates, TCallbacks> KeyboardWithPagination<TObj, TStates, TCallbacks>(
        this ICallbacksBuilder<TObj, TStates, TCallbacks> builder,
        TCallbacks callbackId,
        Func<TObj, uint, ICallbacksBuilderBase<TObj, TStates, TCallbacks>, Task<IFluentPaginationMenu>> paginator) where TObj: IFluentPagination
    {
        var state = builder.GetState();

        async Task Func(TObj o, ICallbacksBuilderBase<TObj, TStates, TCallbacks> b)
        {
            if (o.PageNumber == 0)
            {
                o.PageNumber = 1;
            }
            var currentPage = o.PageNumber;
            var pagination = await paginator(o, currentPage, b);
            var fluentPaginator = new FluentPaginator(pagination, currentPage);
            if (fluentPaginator.PagesCount() > 1)
            {
                Paginate(state.Id, callbackId, b, fluentPaginator);
            }
        }

        return builder.KeyBoard(Func);
    }

    private static void Paginate<TObj, TStates, TCallbacks>(TStates stateId, 
        TCallbacks callbackId,
        ICallbacksBuilderBase<TObj, TStates, TCallbacks> arg2, 
        IPaginationMenu pagination) where TObj: IFluentPagination
    {
        var rowBuilder = arg2.Row();

        Task<TStates> Mover(CallbackQuery _, TObj o, string cd)
        {
            var pageNum = uint.Parse(cd);
            o.PageNumber = pageNum;
            return Task.FromResult(stateId);
        }

        if (!pagination.IsFirstPage())
        {
            if (pagination.IsThirdOrMorePage())
            {
                rowBuilder.OnCallback(callbackId, "1 <<", "1", Mover, true);
            }

            if (pagination.IsSecondOrMorePage())
            {
                rowBuilder.OnCallback(callbackId, $"{pagination.PreviousPageNumber()} <",
                    $"{pagination.PreviousPageNumber()}", Mover, true);
            }
        }

        rowBuilder.OnCallback(callbackId, $"·{pagination.PageNumber}·", $"{pagination.PageNumber}", Mover, true);

        if (!pagination.IsLastPage())
        {
            rowBuilder.OnCallback(callbackId, $"> {pagination.NextPageNumber()}", $"{pagination.NextPageNumber()}",
                Mover, true);

            if (!pagination.IsPenultPage())
            {
                rowBuilder.OnCallback(callbackId, $">> {pagination.PagesCount()}", $"{pagination.PagesCount()}", Mover,
                    true);
            }
        }
    }
}

internal class FluentPaginator: IPaginationMenu
{
    public FluentPaginator(IFluentPaginationMenu pagination, uint currentPage)
    {
        PageNumber = currentPage;
        TotalCount = pagination.TotalCount;
        Limit = pagination.Limit;
    }

    public uint PageNumber { get; }
    public ulong TotalCount { get; }
    public uint Limit { get; }
}
