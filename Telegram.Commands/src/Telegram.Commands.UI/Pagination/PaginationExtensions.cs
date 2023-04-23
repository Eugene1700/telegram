using System;

namespace Telegram.Commands.UI.Pagination
{
    public static class PaginationExtensions 
    {
        public static bool IsFirstPage(this IPaginationMenu pagination)
        {
            return pagination.PageNumber == 1;
        }
        
        public static bool IsMiddlePage(this IPaginationMenu pagination)
        {
            return !pagination.IsFirstPage() && !pagination.IsLastPage();
        }
        
        public static bool IsLastPage(this IPaginationMenu pagination)
        {
            return pagination.PagesCount() == pagination.PageNumber;
        }
        
        public static uint PagesCount(this IPaginationMenu pagination)
        {
            return (uint) Math.Round((double) pagination.TotalCount / pagination.Limit, MidpointRounding.ToPositiveInfinity);
        }
        
        public static bool IsSecondOrMorePage(this IPaginationMenu pagination)
        {
            return pagination.PageNumber > 1;
        }
        
        public static bool IsThirdOrMorePage(this IPaginationMenu pagination)
        {
            return pagination.PageNumber > 2;
        }
        
        public static bool IsPenultPage(this IPaginationMenu pagination)
        {
            return (pagination.PageNumber + 1) == pagination.PagesCount();
        }
        
        public static uint PreviousPageNumber(this IPaginationMenu pagination)
        {
            return pagination.PageNumber <= 1 ? 0 : pagination.PageNumber - 1;
        }
        
        public static uint NextPageNumber(this IPaginationMenu pagination)
        {
            var pagesCount = pagination.PagesCount();
            return pagination.PageNumber == pagesCount
                ? pagesCount
                : pagination.PageNumber + 1;
        }

    }
}