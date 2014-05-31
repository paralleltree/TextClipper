using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace TextClipper.Converters
{
    sealed class StringShortenerConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string str = value as string;
            if (str == null) return string.Empty;

            // 空白を除いた最初の改行まで。ロクなパターン思いつかなかった
            return System.Text.RegularExpressions.Regex.Split(str.Trim(), @"[\r\n]+").First();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
