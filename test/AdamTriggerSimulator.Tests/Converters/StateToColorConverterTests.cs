using System.Globalization;
using System.Windows.Media;
using AdamTriggerSimulator.Converters;
using AdamTriggerSimulator.Models;

namespace AdamTriggerSimulator.Tests.Converters;

public class StateToColorConverterTests
{
    private readonly StateToColorConverter _converter;

    public StateToColorConverterTests()
    {
        _converter = new StateToColorConverter();
    }

    [Fact]
    public void Convert_HighState_ReturnsOrangeBrush()
    {
        // Arrange
        var state = DigitalInputState.High;

        // Act
        var result = _converter.Convert(state, typeof(SolidColorBrush), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.IsType<SolidColorBrush>(result);
        var brush = (SolidColorBrush)result;
        var expectedColor = (Color)ColorConverter.ConvertFromString("#FF9800")!;
        Assert.Equal(expectedColor, brush.Color);
    }

    [Fact]
    public void Convert_LowState_ReturnsBlueBrush()
    {
        // Arrange
        var state = DigitalInputState.Low;

        // Act
        var result = _converter.Convert(state, typeof(SolidColorBrush), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.IsType<SolidColorBrush>(result);
        var brush = (SolidColorBrush)result;
        var expectedColor = (Color)ColorConverter.ConvertFromString("#2196F3")!;
        Assert.Equal(expectedColor, brush.Color);
    }

    [Fact]
    public void Convert_NullValue_ReturnsGrayBrush()
    {
        // Act
        var result = _converter.Convert(null, typeof(SolidColorBrush), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.IsType<SolidColorBrush>(result);
        var brush = (SolidColorBrush)result;
        Assert.Equal(Colors.Gray, brush.Color);
    }

    [Fact]
    public void Convert_InvalidType_ReturnsGrayBrush()
    {
        // Arrange
        var invalidValue = "not a state";

        // Act
        var result = _converter.Convert(invalidValue, typeof(SolidColorBrush), null, CultureInfo.InvariantCulture);

        // Assert
        Assert.IsType<SolidColorBrush>(result);
        var brush = (SolidColorBrush)result;
        Assert.Equal(Colors.Gray, brush.Color);
    }

    [Fact]
    public void ConvertBack_ThrowsNotImplementedException()
    {
        // Act & Assert
        Assert.Throws<NotImplementedException>(() =>
            _converter.ConvertBack(null, typeof(DigitalInputState), null, CultureInfo.InvariantCulture));
    }
}
