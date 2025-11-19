using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace Gradil.Converters
{
    public class FileNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return string.Empty;
            try
            {
                var s = value as string;
                if (string.IsNullOrEmpty(s)) return string.Empty;
                return Path.GetFileName(s);
            }
            catch
            {
                return value.ToString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
