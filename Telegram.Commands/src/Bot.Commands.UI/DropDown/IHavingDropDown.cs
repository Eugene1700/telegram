namespace Bot.Commands.UI.DropDown
{
    public interface IHavingDropDown<TItem>
    {
        DropDown<TItem> DropDown { get; }
    }
}