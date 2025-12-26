using System;
using System.Globalization;
using System.Windows.Data;

namespace ООП_Курсовой.SupportClasses;

public class NullableLongToBoolMultiConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values == null || values.Length < 2)
            return false;

        long? nullableValue = values[0] as long?;
        long? categoryId = values[1] as long?;

        if (categoryId.HasValue)
        {
            return nullableValue.HasValue && nullableValue.Value == categoryId.Value;
        }
        return !nullableValue.HasValue;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        return new object[] { Binding.DoNothing, Binding.DoNothing };
    }
}