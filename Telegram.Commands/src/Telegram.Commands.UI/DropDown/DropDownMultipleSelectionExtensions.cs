namespace Telegram.Commands.UI.DropDown
{
    public static class DropDownMultipleSelectionExtensions
    {
        public static void AddItem<T>(this IDropDownMultipleSelection<T> dropDown, T newItem)
        {
            if (!dropDown.SelectedItems.Contains(newItem))
            {
                dropDown.SelectedItems.Add(newItem);
            }
        }
        
        public static void RemoveItem<T>(this IDropDownMultipleSelection<T> dropDown, T item)
        {
            if (dropDown.SelectedItems.Contains(item))
            {
                dropDown.SelectedItems.Remove(item);
            }
        }

        public static void AddOrRemoveItem<T>(this IDropDownMultipleSelection<T> dropDown, T item)
        {
            if (dropDown.SelectedItems.Contains(item))
            {
                dropDown.SelectedItems.Remove(item);
            }
            else
            {
                dropDown.SelectedItems.Add(item);
            }
        }
        
        public static string GetDropDownPrefix<T>(this IDropDown<T> dropDown)
        {
            const string showPrefix = "∨";
            const string hidePrefix = "∧";
            return dropDown.Show ? showPrefix : hidePrefix;
        }
    }
}