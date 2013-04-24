using System.ComponentModel;
using System.Windows;

namespace PHmiClient.Utils
{
    public static class InDesignModeHelper
    {
        private static bool? _isInDesignMode;

        public static bool IsInDesignMode
        {
            get
            {
                if (!_isInDesignMode.HasValue)

                {
#if SILVERLIGHT
                    _isInDesignMode = DesignerProperties.IsInDesignTool;
#else
                    var prop = DesignerProperties.IsInDesignModeProperty;
                    _isInDesignMode
                        = (bool) DependencyPropertyDescriptor
                                     .FromProperty(prop, typeof (FrameworkElement))
                                     .Metadata.DefaultValue;
#endif
                }
                return _isInDesignMode.Value;
            }
        }
    }
}
