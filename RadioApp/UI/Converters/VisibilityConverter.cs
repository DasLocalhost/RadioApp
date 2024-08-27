using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace RadioApp.UI.Converters
{
    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not bool bvalue)
                return Visibility.Visible;

            bool invert = false;

            if (parameter != null && parameter.ToString() == "invert")
                invert = true;

            if (bvalue)
                return invert ? Visibility.Visible : Visibility.Collapsed;
            else
                return invert ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
