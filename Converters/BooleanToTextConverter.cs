using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace CultivoApp.Converters
{
    public class BooleanToTextConverter : IValueConverter
    {
        public static readonly BooleanToTextConverter Instance = new();

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool b)
                return b ? "Ligado" : "Desligado";
            return "Desligado";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string s)
                return s.Equals("Ligado", StringComparison.OrdinalIgnoreCase);
            return false;
        }
    }
}
