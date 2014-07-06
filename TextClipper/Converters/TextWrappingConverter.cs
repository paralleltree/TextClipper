using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace TextClipper.Converters
{
    class TextWrappingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((bool)value)
                return TextWrapping.Wrap;
            else
                return TextWrapping.NoWrap;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((TextWrapping)value == TextWrapping.Wrap)
                return true;
            else
                return false;
        }
    }
}
