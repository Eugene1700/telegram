namespace Bot.Commands.UI.Pagination
{
    public interface IFluentPaginationMenu
    {
        public ulong TotalCount { get; }
        public uint Limit { get; }
    }
}