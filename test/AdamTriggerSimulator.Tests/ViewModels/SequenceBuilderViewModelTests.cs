using AdamTriggerSimulator.Models;
using AdamTriggerSimulator.Services;
using AdamTriggerSimulator.ViewModels;
using Moq;

namespace AdamTriggerSimulator.Tests.ViewModels;

public class SequenceBuilderViewModelTests
{
    private readonly Mock<IAdamCommunicationService> _mockCommunicationService;
    private readonly List<CommandLogEntry> _commandLog;
    private readonly SequenceBuilderViewModel _viewModel;

    public SequenceBuilderViewModelTests()
    {
        _mockCommunicationService = new Mock<IAdamCommunicationService>();
        _commandLog = new List<CommandLogEntry>();
        _viewModel = new SequenceBuilderViewModel(
            _mockCommunicationService.Object,
            logEntry => _commandLog.Add(logEntry));
    }

    [Fact]
    public void NewSequence_CreatesEmptySequence()
    {
        // Act
        _viewModel.NewSequenceCommand.Execute(null);

        // Assert
        Assert.NotNull(_viewModel.CurrentSequence);
        Assert.Equal("New Sequence", _viewModel.SequenceName);
        Assert.Equal(1, _viewModel.LoopCount);
        Assert.False(_viewModel.IsInfiniteLoop);
        Assert.Empty(_viewModel.Actions);
    }

    [Fact]
    public void AddSetHighAction_AddsActionToSequence()
    {
        // Arrange
        var inputChannel = 0;

        // Act
        _viewModel.AddSetHighActionCommand.Execute(inputChannel);

        // Assert
        Assert.Single(_viewModel.Actions);
        Assert.IsType<SetInputHighAction>(_viewModel.Actions[0].Model);
        var action = (SetInputHighAction)_viewModel.Actions[0].Model;
        Assert.Equal(inputChannel, action.InputChannel);
    }

    [Fact]
    public void AddSetLowAction_AddsActionToSequence()
    {
        // Arrange
        var inputChannel = 2;

        // Act
        _viewModel.AddSetLowActionCommand.Execute(inputChannel);

        // Assert
        Assert.Single(_viewModel.Actions);
        Assert.IsType<SetInputLowAction>(_viewModel.Actions[0].Model);
        var action = (SetInputLowAction)_viewModel.Actions[0].Model;
        Assert.Equal(inputChannel, action.InputChannel);
    }

    [Fact]
    public void AddDelayAction_AddsActionToSequence()
    {
        // Arrange
        var durationMs = 1000;

        // Act
        _viewModel.AddDelayActionCommand.Execute(durationMs);

        // Assert
        Assert.Single(_viewModel.Actions);
        Assert.IsType<DelayAction>(_viewModel.Actions[0].Model);
        var action = (DelayAction)_viewModel.Actions[0].Model;
        Assert.Equal(durationMs, action.DurationMs);
    }

    [Fact]
    public void RemoveAction_RemovesSelectedAction()
    {
        // Arrange
        _viewModel.AddSetHighActionCommand.Execute(0);
        _viewModel.AddSetLowActionCommand.Execute(1);
        _viewModel.SelectedAction = _viewModel.Actions[0];

        // Act
        _viewModel.RemoveActionCommand.Execute(null);

        // Assert
        Assert.Single(_viewModel.Actions);
        Assert.IsType<SetInputLowAction>(_viewModel.Actions[0].Model);
    }

    [Fact]
    public void MoveActionUp_MovesActionUpInSequence()
    {
        // Arrange
        _viewModel.AddSetHighActionCommand.Execute(0);
        _viewModel.AddSetLowActionCommand.Execute(1);
        _viewModel.SelectedAction = _viewModel.Actions[1];

        // Act
        _viewModel.MoveActionUpCommand.Execute(null);

        // Assert
        Assert.Equal(2, _viewModel.Actions.Count);
        Assert.IsType<SetInputLowAction>(_viewModel.Actions[0].Model);
        Assert.IsType<SetInputHighAction>(_viewModel.Actions[1].Model);
    }

    [Fact]
    public void MoveActionDown_MovesActionDownInSequence()
    {
        // Arrange
        _viewModel.AddSetHighActionCommand.Execute(0);
        _viewModel.AddSetLowActionCommand.Execute(1);
        _viewModel.SelectedAction = _viewModel.Actions[0];

        // Act
        _viewModel.MoveActionDownCommand.Execute(null);

        // Assert
        Assert.Equal(2, _viewModel.Actions.Count);
        Assert.IsType<SetInputLowAction>(_viewModel.Actions[0].Model);
        Assert.IsType<SetInputHighAction>(_viewModel.Actions[1].Model);
    }

    [Fact]
    public void SaveSequence_UpdatesSequenceProperties()
    {
        // Arrange
        _viewModel.NewSequenceCommand.Execute(null);
        _viewModel.SequenceName = "Test Sequence";
        _viewModel.LoopCount = 5;
        _viewModel.IsInfiniteLoop = false;
        _viewModel.AddSetHighActionCommand.Execute(0);
        _viewModel.AddDelayActionCommand.Execute(1000);

        // Act
        var savedSequence = _viewModel.SaveSequence();

        // Assert
        Assert.Equal("Test Sequence", savedSequence.Name);
        Assert.Equal(5, savedSequence.LoopCount);
        Assert.Equal(2, savedSequence.Actions.Count);
    }

    [Fact]
    public void SaveSequence_InfiniteLoop_SetsLoopCountToZero()
    {
        // Arrange
        _viewModel.NewSequenceCommand.Execute(null);
        _viewModel.SequenceName = "Infinite Test";
        _viewModel.IsInfiniteLoop = true;
        _viewModel.AddSetHighActionCommand.Execute(0);

        // Act
        var savedSequence = _viewModel.SaveSequence();

        // Assert
        Assert.Equal(0, savedSequence.LoopCount);
    }

    [Fact]
    public void LoadSequence_LoadsAllProperties()
    {
        // Arrange
        var sequence = new TriggerSequence
        {
            Name = "Loaded Sequence",
            LoopCount = 3,
            Actions = new List<SequenceAction>
            {
                new SetInputHighAction { InputChannel = 1 },
                new DelayAction { DurationMs = 2000 },
                new SetInputLowAction { InputChannel = 1 }
            }
        };

        // Act
        _viewModel.LoadSequence(sequence);

        // Assert
        Assert.Equal("Loaded Sequence", _viewModel.SequenceName);
        Assert.Equal(3, _viewModel.LoopCount);
        Assert.False(_viewModel.IsInfiniteLoop);
        Assert.Equal(3, _viewModel.Actions.Count);
    }

    [Fact]
    public void LoadSequence_ZeroLoopCount_SetsInfiniteLoop()
    {
        // Arrange
        var sequence = new TriggerSequence
        {
            Name = "Infinite Sequence",
            LoopCount = 0,
            Actions = new List<SequenceAction>
            {
                new SetInputHighAction { InputChannel = 0 }
            }
        };

        // Act
        _viewModel.LoadSequence(sequence);

        // Assert
        Assert.True(_viewModel.IsInfiniteLoop);
        Assert.Equal(1, _viewModel.LoopCount); // Should be 1 when infinite is on
    }

    [Fact]
    public void UpdateProfile_SetsCurrentProfile()
    {
        // Arrange
        var profile = new DeviceProfile
        {
            Name = "Test Device",
            IpAddress = "192.168.1.100",
            Port = 1025
        };

        // Act
        _viewModel.UpdateProfile(profile);

        // Assert
        Assert.Equal(profile, _viewModel.CurrentProfile);
    }
}
