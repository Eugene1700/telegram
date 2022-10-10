using System.Collections.Generic;

namespace Telegram.Commands.UI.DropDown
{
    public interface IDropDownMultipleSelection<T> : IDropDown<HashSet<T>>
    {
        
    }
}