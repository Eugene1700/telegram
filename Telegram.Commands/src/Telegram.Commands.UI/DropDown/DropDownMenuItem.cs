namespace Telegram.Commands.UI.DropDown
{
    public class DropDownMenuItem<TITem>
    {
        public TITem Item { get; set; }
        public string Title { get; set; }
        public bool Selected { get; set; }
        public string CallbackData { get; set; }
    }
}