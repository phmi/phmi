namespace PHmiClient.Controls.Pages
{
    public interface IPage
    {
        IRoot Root { get; set; }
        object PageName { get; }
    }
}
