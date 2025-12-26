using System.Globalization;
using System.Windows.Data;

namespace ООП_Курсовой.SupportClasses;

public class EnumToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
            return false;

        string checkValue = value.ToString() ?? "";
        string targetValue = parameter.ToString() ?? "";
        
        return checkValue.Equals(targetValue, StringComparison.InvariantCultureIgnoreCase);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue && boolValue && parameter != null)
        {
            string targetValue = parameter.ToString() ?? "";
            if (Enum.IsDefined(targetType, targetValue))
            {
                return Enum.Parse(targetType, targetValue, true);
            }
        }
        return Binding.DoNothing;
    }
}