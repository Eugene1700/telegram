namespace Telegram.Commands.UI.DropDown
{
    public interface IDropDown<T> : IShow
    {
        public T SelectedItems { get; set; }
    }

    public interface IShow
    {
        bool Show { get; set; }
    }
}