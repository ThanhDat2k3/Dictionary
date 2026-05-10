using System;
using System.Globalization;
using System.Windows.Data;

namespace Dictionary.Converters
{
    public class TruncateTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string text)
                return string.Empty;

            if (!int.TryParse(parameter?.ToString() ?? "100", out int maxLength))
                maxLength = 100;

            if (text.Length <= maxLength)
                return text;

            return text.Substring(0, maxLength).TrimEnd() + "...";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
