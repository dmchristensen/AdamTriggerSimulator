using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using AdamTriggerSimulator.Models;

namespace AdamTriggerSimulator.Converters;

/// <summary>
/// Converts DigitalInputState to a color brush for visual indication.
/// </summary>
public class StateToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DigitalInputState state)
        {
            return state == DigitalInputState.High
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9800")!) // Orange for HIGH
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2196F3")!); // Blue for LOW
        }

        return new SolidColorBrush(Colors.Gray);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
