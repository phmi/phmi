using System.Windows;

namespace PHmiClient.Utils
{
    public class ClipboardHelper : IClipboardHelper
    {
        public string GetText()
        {
            return Clipboard.ContainsText() ? Clipboard.GetText() : string.Empty;
        }

        public void SetText(string text)
        {
            Clipboard.SetText(text);
        }
    }
}
