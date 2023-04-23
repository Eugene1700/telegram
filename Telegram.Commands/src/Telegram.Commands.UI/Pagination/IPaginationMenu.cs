namespace Telegram.Commands.UI.Pagination
{
    public interface IFluentPaginationMenu
    {
        public ulong TotalCount { get; }
        public uint Limit { get; }
    }
    
    public interface IFluentPagination
    {
        public uint PageNumber { get; set; }
    }
    
    public interface IPaginationMenu
    {
        public uint PageNumber { get; }
        
        public ulong TotalCount { get; }
        public uint Limit { get; }
    }
}