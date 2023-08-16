using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Commands.Core.Fluent.Builders.CallbackBuilders;
using Telegram.Commands.Core.Fluent.Builders.CallbackBuilders.Extensions;

namespace Telegram.Commands.UI.Pagination
{
    public static class FluentCommandPaginationMenu
    {
        public static ICallbacksBuilder<TObj, TStates> KeyboardWithPagination<TObj, TStates>(
            this ICallbacksBuilder<TObj, TStates> builder,
            Func<TObj, uint, ICallbacksBuilderBase<TObj, TStates>, Task<IFluentPaginationMenu>> paginator) where TObj: IFluentPagination
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
                    Paginate(state, b, fluentPaginator);
                }
            }

            return builder.KeyBoard(Func);
        }

        private static void Paginate<TObj, TStates>(TStates stateId, 
            ICallbacksBuilderBase<TObj, TStates> arg2, 
            IPaginationMenu pagination) where TObj: IFluentPagination
        {
            var rowBuilder = arg2.Row();

            Task<TStates> Mover(CallbackQuery _, TStates state, TObj o, string cd)
            {
                var pageNum = uint.Parse(cd);
                o.PageNumber = pageNum;
                return Task.FromResult(stateId);
            }

            if (!pagination.IsFirstPage())
            {
                if (pagination.IsThirdOrMorePage())
                {
                    rowBuilder.OnCallback("1 <<", "1", Mover, true);
                }

                if (pagination.IsSecondOrMorePage())
                {
                    rowBuilder.OnCallback($"{pagination.PreviousPageNumber()} <",
                        $"{pagination.PreviousPageNumber()}", Mover, true);
                }
            }

            rowBuilder.OnCallback($"·{pagination.PageNumber}·", $"{pagination.PageNumber}", Mover, true);

            if (!pagination.IsLastPage())
            {
                rowBuilder.OnCallback($"> {pagination.NextPageNumber()}", $"{pagination.NextPageNumber()}",
                    Mover, true);

                if (!pagination.IsPenultPage())
                {
                    rowBuilder.OnCallback($">> {pagination.PagesCount()}", $"{pagination.PagesCount()}", Mover,
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
