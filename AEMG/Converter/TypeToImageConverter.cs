using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace AEMG
{
    [ValueConversion(typeof(MacroItemType), typeof(BitmapImage))]
    public class TypeToImageConverter : IValueConverter
    {
        public static TypeToImageConverter Instance = new TypeToImageConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var macro = (MacroItemType)value;

            string image = "";

            switch (macro)
            {
                case MacroItemType.EXP:
                    image = "exp.png";
                    break;
                case MacroItemType.AD:
                    image = "green_key.png";
                    break;
                case MacroItemType.ADVH:
                    image = "red_key.png";
                    break;
            }

            return new BitmapImage(new Uri($"pack://Application:,,,/Images/{image}"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
