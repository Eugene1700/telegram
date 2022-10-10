using Telegram.Commands.UI.DropDown;

namespace SimpleHandlers.Services.Commands.Models
{
    public class DropDownSessionObject : IHavingDropDown<string>
    {
        public DropDown<string> DropDown { get; set; }
    }
}