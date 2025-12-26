using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ООП_Курсовой.SupportClasses;

public class NullableLongToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        long? nullableValue = value as long?;
        if (nullableValue.HasValue || value == null)
        {
            if (parameter is DependencyObject depObj && depObj != null)
            {
                var tagValue = depObj.GetValue(FrameworkElement.TagProperty);
                if (tagValue is long tagLong)
                {
                    return nullableValue.HasValue && nullableValue.Value == tagLong;
                }
            }
            
            if (parameter is long paramValue)
            {
                if (paramValue == -1)
                    return !nullableValue.HasValue;
                return nullableValue.HasValue && nullableValue.Value == paramValue;
            }
            if (parameter == null || (parameter is string str && str == "null"))
            {
                return !nullableValue.HasValue;
            }
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue && boolValue)
        {
            if (parameter is DependencyObject depObj && depObj != null)
            {
                var tagValue = depObj.GetValue(FrameworkElement.TagProperty);
                if (tagValue is long tagLong)
                {
                    return tagLong;
                }
            }
            
            if (parameter is long paramValue)
            {
                if (paramValue == -1)
                    return null;
                return paramValue;
            }
            if (parameter == null || (parameter is string str && str == "null"))
            {
                return null;
            }
        }
        return null;
    }
}

