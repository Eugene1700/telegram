namespace Telegram.Commands.UI.Pagination
{
    public interface IFluentPaginationMenu
    {
        public long TotalCount { get; }
        public int Limit { get; }
    }
    
    public interface IFluentPagination
    {
        public int PageNumber { get; set; }
    }
    
    public interface IPaginationMenu
    {
        public int PageNumber { get; }
        
        public long TotalCount { get; }
        public int Limit { get; }
    }
}