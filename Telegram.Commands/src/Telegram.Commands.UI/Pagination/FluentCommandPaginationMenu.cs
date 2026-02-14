using System;
using System.Threading.Tasks;
using Telegram.Commands.Core.Fluent.Builders.CallbackBuilders;
using Telegram.Commands.Core.Fluent.Builders.CallbackBuilders.Extensions;

namespace Telegram.Commands.UI.Pagination
{
    public static class FluentCommandPaginationMenu
    {
        public static ICallbacksBuilder<TObj, TStates> KeyboardWithPagination<TObj, TStates, TQuery>(
            this ICallbacksBuilder<TObj, TStates> builder,
            Func<TObj, uint, ICallbacksBuilderBase<TObj, TStates>, Task<IFluentPaginationMenu>> paginator) where TObj: IFluentPagination where TQuery : class
        {
            async Task Func(TStates state, TObj o, ICallbacksBuilderBase<TObj, TStates> b)
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
                    Paginate<TObj, TStates, TQuery>(state, b, fluentPaginator);
                }
            }

            return builder.KeyBoard(Func);
        }

        private static void Paginate<TObj, TStates, TQuery>(TStates stateId, 
            ICallbacksBuilderBase<TObj, TStates> arg2, 
            IPaginationMenu pagination) where TObj: IFluentPagination where TQuery : class
        {
            var rowBuilder = arg2.Row();

            Task<TStates> Mover(TQuery _, TStates state, TObj o, string cd)
            {
                var pageNum = uint.Parse(cd);
                o.PageNumber = pageNum;
                return Task.FromResult(stateId);
            }

            if (!pagination.IsFirstPage())
            {
                if (pagination.IsThirdOrMorePage())
                {
                    rowBuilder.OnCallback<TObj, TStates, TQuery>("1 <<", "1", Mover, true);
                }

                if (pagination.IsSecondOrMorePage())
                {
                    rowBuilder.OnCallback<TObj, TStates, TQuery>($"{pagination.PreviousPageNumber()} <",
                        $"{pagination.PreviousPageNumber()}", Mover, true);
                }
            }

            rowBuilder.OnCallback<TObj, TStates, TQuery>($"·{pagination.PageNumber}·", $"{pagination.PageNumber}", Mover, true);

            if (!pagination.IsLastPage())
            {
                rowBuilder.OnCallback<TObj, TStates, TQuery>($"> {pagination.NextPageNumber()}", $"{pagination.NextPageNumber()}",
                    Mover, true);

                if (!pagination.IsPenultPage())
                {
                    rowBuilder.OnCallback<TObj, TStates, TQuery>($">> {pagination.PagesCount()}", $"{pagination.PagesCount()}", Mover,
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
}
