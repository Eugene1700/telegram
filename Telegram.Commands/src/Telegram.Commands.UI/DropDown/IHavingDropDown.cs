namespace Telegram.Commands.UI.DropDown
{
    public interface IHavingDropDown<TItem>
    {
        DropDown<TItem> DropDown { get; }
    }
}