using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PHmiClient.Utils
{
    public static class IconHelper
    {
        public static ImageSource GetIcon(Uri uri)
        {
            var ibd = new IconBitmapDecoder(
                uri,
                BitmapCreateOptions.None,
                BitmapCacheOption.Default);
            return ibd.Frames[0];   
        }
    }
}
