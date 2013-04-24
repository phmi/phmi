using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PHmiClient.Utils
{
    public static class ImageHelper
    {
        private static readonly Dictionary<ImageFormat, Type> FormatEncoders = new Dictionary<ImageFormat, Type>();

        static ImageHelper()
        {
            FormatEncoders.Add(ImageFormat.Bmp, typeof(BmpBitmapEncoder));
            FormatEncoders.Add(ImageFormat.Gif, typeof(GifBitmapEncoder));
            FormatEncoders.Add(ImageFormat.Jpeg, typeof(JpegBitmapEncoder));
            FormatEncoders.Add(ImageFormat.Png, typeof(PngBitmapEncoder));
            FormatEncoders.Add(ImageFormat.Tiff, typeof(TiffBitmapEncoder));
        }

        public static ImageSource ToImage(byte[] value)
        {
            try
            {
                if (value != null && value.Length > 0)
                {
                    var stream = new MemoryStream(value);
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.StreamSource = stream;
                    image.EndInit();
                    return image;
                }
            }
            catch (Exception)
            {
                return null;
            }
            return null;
        }

        public static void Save(this BitmapImage imageSource, ImageFormat imageFormat, Stream stream)
        {
            if (imageSource == null)
                return;

            var encoder = (BitmapEncoder)Activator.CreateInstance(FormatEncoders[imageFormat]);
            encoder.Frames.Add(BitmapFrame.Create(imageSource));
            encoder.Save(stream);
        }

        public static byte[] ToBytes(this BitmapImage imageSource, ImageFormat imageFormat)
        {
            if (imageSource == null)
                return null;

            var ms = new MemoryStream();
            imageSource.Save(imageFormat, ms);
            return ms.ToArray();
        }
    }
}
