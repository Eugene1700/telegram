using System.Linq;

namespace Telegram.Commands.UI.DropDown
{
    public class DropDown<TItem>
    {
        public DropDownMenuItem<TItem>[] Items { get; set; }
        public bool NothingIsAll { get; set; }
        public string Title { get; set; }
        public string CallbackData { get; set; }
        public bool Show { get; set; }
        
        public DropDownMode Mode { get; set; }
        
        public void SelectItem(string callback)
        {
            if (Mode == DropDownMode.Single)
            {
                foreach (var dropDownMenuItem in Items.Where(x=>x.CallbackData != callback))
                {
                    dropDownMenuItem.Selected = false;
                }
            }
            
            var item = Items.Single(x => x.CallbackData == callback);
            item.Selected = !item.Selected;
        }
        
        public string GetDropDownPrefix()
        {
            const string showPrefix = "∨";
            const string hidePrefix = "∧";
            return Show ? showPrefix : hidePrefix;
        }

        public bool IsAllSelected()
        {
            var notAnySelected = Items.All(x => !x.Selected);
            var allSelected = Items.All(x => x.Selected);
            return NothingIsAll ? notAnySelected || allSelected : allSelected;
        }
        
        public DropDownMenuItem<TItem>[] GetSelected()
        {
            return Items.Where(x=>x.Selected).ToArray();
        }
    }

    public enum DropDownMode
    {
        Single,
        Multiple
    }
}