using System.Windows.Markup;

namespace PHmiClient.Controls
{
    [ContentProperty("Value")]
    public class DisplayItem
    {
        public object DisplayValue { get; set; }

        public object Value { get; set; }
    }
}
