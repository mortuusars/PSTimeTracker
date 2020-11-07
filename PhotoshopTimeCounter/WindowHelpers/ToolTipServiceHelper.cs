using System.Windows;
using System.Windows.Controls;

namespace PhotoshopTimeCounter.WindowHelpers
{
    public static class ToolTipServiceHelper
    {
        static ToolTipServiceHelper()
        {
            ToolTipService.InitialShowDelayProperty
                .OverrideMetadata(typeof(FrameworkElement),
                                  new FrameworkPropertyMetadata(ToolTipService.ShowDurationProperty.DefaultMetadata.DefaultValue, 
                                  FrameworkPropertyMetadataOptions.Inherits));
        }
    }
}
