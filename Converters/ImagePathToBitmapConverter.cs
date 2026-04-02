using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using System.IO;

namespace CultivoApp.Converters
{
    public class ImagePathToBitmapConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string path && !string.IsNullOrWhiteSpace(path))
            {
                try
                {
                    if (path.StartsWith("file:///"))
                        path = path.Replace("file:///", "").Replace("/", "\\");

                    if (File.Exists(path))
                        return new Bitmap(path);
                }
                catch { }
            }

            // Se não encontrar, mostra imagem padrão
            string defaultPath = Path.Combine(AppContext.BaseDirectory, "Assets", "default.png");
            if (File.Exists(defaultPath))
                return new Bitmap(defaultPath);

            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
