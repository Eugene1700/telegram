namespace Bot.Commands.UI.Pagination
{
    
    public interface IPaginationMenu
    {
        public uint PageNumber { get; }
        
        public ulong TotalCount { get; }
        public uint Limit { get; }
    }
}