using System;

namespace PHmiResources
{
    public static class ImagesUries
    {
        private static Uri GetImageUri(string image)
        {
            return new Uri(string.Format(@"pack://application:,,,/PHmiResources;component/Images/{0}", image), UriKind.Absolute);
        }

        public static Uri AddUserIco
        {
            get { return GetImageUri("add_user.ico"); }
        }

        public static Uri EditUserIco
        {
            get { return GetImageUri("edit_user.ico"); }
        }

        public static Uri FolderOpenPng
        {
            get { return GetImageUri("folder_open.png"); }
        }

        public static Uri FolderClosedPng
        {
            get { return GetImageUri("folder_closed.png"); }
        }

        public static Uri InputDevicePng
        {
            get { return GetImageUri("input_device.png"); }
        }

        public static Uri InputDeviceIco
        {
            get { return GetImageUri("input_device.ico"); }
        }
    }
}
