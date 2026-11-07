using Bot.Commands.Abstract.Commands;
using System;
using System.Threading.Tasks;
using Bot.Commands.Core.Fluent.Builders.CallbackBuilders;
using Bot.Commands.Core.Fluent.Builders.CallbackBuilders.Extensions;

namespace Bot.Commands.UI.Pagination
{

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
