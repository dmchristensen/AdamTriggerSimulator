using AdamTriggerSimulator.Models;

namespace AdamTriggerSimulator.Tests.Models;

public class SequenceActionTests
{
    [Theory]
    [InlineData(0, "DI0 → HIGH")]
    [InlineData(1, "DI1 → HIGH")]
    [InlineData(5, "DI5 → HIGH")]
    public void SetInputHighAction_GetDescription_ReturnsCorrectFormat(int channel, string expected)
    {
        // Arrange
        var action = new SetInputHighAction { InputChannel = channel };

        // Act
        var description = action.GetDescription();

        // Assert
        Assert.Equal(expected, description);
    }

    [Theory]
    [InlineData(0, "DI0 → LOW")]
    [InlineData(2, "DI2 → LOW")]
    [InlineData(4, "DI4 → LOW")]
    public void SetInputLowAction_GetDescription_ReturnsCorrectFormat(int channel, string expected)
    {
        // Arrange
        var action = new SetInputLowAction { InputChannel = channel };

        // Act
        var description = action.GetDescription();

        // Assert
        Assert.Equal(expected, description);
    }

    [Theory]
    [InlineData(1000, "Delay 1s")]
    [InlineData(2000, "Delay 2s")]
    [InlineData(5000, "Delay 5s")]
    [InlineData(10000, "Delay 10s")]
    [InlineData(500, "Delay 500ms")]
    [InlineData(1500, "Delay 1.5s")]
    public void DelayAction_GetDescription_ReturnsCorrectFormat(int durationMs, string expected)
    {
        // Arrange
        var action = new DelayAction { DurationMs = durationMs };

        // Act
        var description = action.GetDescription();

        // Assert
        Assert.Equal(expected, description);
    }

    [Fact]
    public void TriggerSequence_DefaultConstructor_InitializesCollections()
    {
        // Act
        var sequence = new TriggerSequence();

        // Assert
        Assert.NotNull(sequence.Actions);
        Assert.Empty(sequence.Actions);
    }

    [Fact]
    public void TriggerSequence_SetProperties_StoresValuesCorrectly()
    {
        // Arrange
        var sequence = new TriggerSequence();
        var testDate = new DateTime(2024, 1, 1);

        // Act
        sequence.Id = Guid.NewGuid();
        sequence.Name = "Test Sequence";
        sequence.Description = "Test Description";
        sequence.LoopCount = 5;
        sequence.CreatedAt = testDate;
        sequence.LastModified = testDate;

        // Assert
        Assert.NotEqual(Guid.Empty, sequence.Id);
        Assert.Equal("Test Sequence", sequence.Name);
        Assert.Equal("Test Description", sequence.Description);
        Assert.Equal(5, sequence.LoopCount);
        Assert.Equal(testDate, sequence.CreatedAt);
        Assert.Equal(testDate, sequence.LastModified);
    }

    [Fact]
    public void DeviceProfile_DefaultConstructor_InitializesWithDefaults()
    {
        // Act
        var profile = new DeviceProfile();

        // Assert
        Assert.NotEqual(Guid.Empty, profile.Id);
        Assert.Equal(1025, profile.Port);
    }

    [Fact]
    public void DeviceProfile_SetProperties_StoresValuesCorrectly()
    {
        // Arrange
        var profile = new DeviceProfile();

        // Act
        profile.Name = "Test Device";
        profile.IpAddress = "192.168.1.100";
        profile.Port = 502;

        // Assert
        Assert.Equal("Test Device", profile.Name);
        Assert.Equal("192.168.1.100", profile.IpAddress);
        Assert.Equal(502, profile.Port);
    }
}
