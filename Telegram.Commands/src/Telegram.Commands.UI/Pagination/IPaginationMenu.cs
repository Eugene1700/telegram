namespace Telegram.Commands.UI.Pagination
{
    public interface IPaginationMenu
    {
        public long TotalCount { get; }
        public int Limit { get; }
        public int PageNumber { get; }
    }
}