using System.Globalization;
using System.Windows.Data;

namespace ООП_Курсовой.SupportClasses;

public class StringToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string strValue && parameter is string paramValue)
        {
            return strValue == paramValue;
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue && boolValue && parameter is string paramValue)
        {
            return paramValue;
        }
        return null!;
    }
}

