using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AdamTriggerSimulator.Models;
using AdamTriggerSimulator.Services;

namespace AdamTriggerSimulator.ViewModels;

/// <summary>
/// ViewModel for controlling individual digital inputs.
/// </summary>
public partial class InputControlViewModel : ViewModelBase
{
    private readonly IAdamCommunicationService _communicationService;
    private readonly Action<CommandLogEntry> _onCommandExecuted;

    [ObservableProperty]
    private int _inputChannel;

    [ObservableProperty]
    private DigitalInputState _currentState = DigitalInputState.Low;

    [ObservableProperty]
    private bool _isExecuting;

    [ObservableProperty]
    private DeviceProfile? _currentProfile;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SetHighCommand))]
    [NotifyCanExecuteChangedFor(nameof(SetLowCommand))]
    private bool _isConnected;

    /// <summary>
    /// Display name for this input (e.g., "DI0").
    /// </summary>
    public string DisplayName => $"DI{InputChannel}";

    public InputControlViewModel(
        int inputChannel,
        IAdamCommunicationService communicationService,
        Action<CommandLogEntry> onCommandExecuted)
    {
        _inputChannel = inputChannel;
        _communicationService = communicationService;
        _onCommandExecuted = onCommandExecuted;
    }

    /// <summary>
    /// Sets this input to HIGH state.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExecuteCommands))]
    private async Task SetHighAsync()
    {
        if (CurrentProfile == null)
            return;

        IsExecuting = true;
        try
        {
            var logEntry = await _communicationService.SetInputStateAsync(
                CurrentProfile,
                InputChannel,
                DigitalInputState.High);

            if (logEntry.Success)
            {
                CurrentState = DigitalInputState.High;
            }

            _onCommandExecuted(logEntry);
        }
        finally
        {
            IsExecuting = false;
        }
    }

    /// <summary>
    /// Sets this input to LOW state.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExecuteCommands))]
    private async Task SetLowAsync()
    {
        if (CurrentProfile == null)
            return;

        IsExecuting = true;
        try
        {
            var logEntry = await _communicationService.SetInputStateAsync(
                CurrentProfile,
                InputChannel,
                DigitalInputState.Low);

            if (logEntry.Success)
            {
                CurrentState = DigitalInputState.Low;
            }

            _onCommandExecuted(logEntry);
        }
        finally
        {
            IsExecuting = false;
        }
    }

    /// <summary>
    /// Updates the current device profile for this control.
    /// </summary>
    public void UpdateProfile(DeviceProfile? profile)
    {
        CurrentProfile = profile;
    }

    /// <summary>
    /// Updates the current state of this input (used when setting all inputs at once).
    /// </summary>
    public void UpdateState(DigitalInputState state)
    {
        CurrentState = state;
    }

    /// <summary>
    /// Determines if commands can execute (connected and not already executing).
    /// </summary>
    private bool CanExecuteCommands() => IsConnected && !IsExecuting;
}
