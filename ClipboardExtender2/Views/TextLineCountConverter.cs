using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardExtender2.Views
{
    public class TextLineCountConverter: System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value.ToString()
                .TrimEnd(new char[] { '\r', '\n'})
                .Split(new string[] {"\r\n", "\r", "\n" }, StringSplitOptions.None)
                .Count();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
