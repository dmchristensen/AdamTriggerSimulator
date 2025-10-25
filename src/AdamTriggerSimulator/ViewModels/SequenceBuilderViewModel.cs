using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AdamTriggerSimulator.Models;
using AdamTriggerSimulator.Services;

namespace AdamTriggerSimulator.ViewModels;

/// <summary>
/// ViewModel for building and executing trigger sequences.
/// </summary>
public partial class SequenceBuilderViewModel : ViewModelBase
{
    private readonly IAdamCommunicationService _communicationService;
    private readonly Action<CommandLogEntry> _onCommandExecuted;
    private readonly Action<int, DigitalInputState>? _onInputStateChanged;
    private CancellationTokenSource? _cancellationTokenSource;

    [ObservableProperty]
    private TriggerSequence? _currentSequence;

    [ObservableProperty]
    private ObservableCollection<SequenceActionViewModel> _actions = new();

    [ObservableProperty]
    private SequenceActionViewModel? _selectedAction;

    [ObservableProperty]
    private bool _isExecuting;

    [ObservableProperty]
    private DeviceProfile? _currentProfile;

    [ObservableProperty]
    private string _sequenceName = "New Sequence";

    [ObservableProperty]
    private int _loopCount = 1;

    [ObservableProperty]
    private bool _isInfiniteLoop;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PlaySequenceCommand))]
    private bool _isConnected;

    public SequenceBuilderViewModel(
        IAdamCommunicationService communicationService,
        Action<CommandLogEntry> onCommandExecuted,
        Action<int, DigitalInputState>? onInputStateChanged = null)
    {
        _communicationService = communicationService;
        _onCommandExecuted = onCommandExecuted;
        _onInputStateChanged = onInputStateChanged;
    }

    /// <summary>
    /// Loads a sequence for editing.
    /// </summary>
    public void LoadSequence(TriggerSequence sequence)
    {
        CurrentSequence = sequence;
        SequenceName = sequence.Name;
        LoopCount = sequence.LoopCount == 0 ? 1 : sequence.LoopCount;
        IsInfiniteLoop = sequence.LoopCount == 0;

        Actions.Clear();
        foreach (var action in sequence.Actions)
        {
            Actions.Add(new SequenceActionViewModel(action));
        }
    }

    /// <summary>
    /// Creates a new empty sequence.
    /// </summary>
    [RelayCommand]
    public void NewSequence()
    {
        CurrentSequence = new TriggerSequence
        {
            Name = "New Sequence"
        };
        SequenceName = "New Sequence";
        LoopCount = 1;
        IsInfiniteLoop = false;
        Actions.Clear();
    }

    /// <summary>
    /// Adds a SetHigh action to the sequence.
    /// </summary>
    [RelayCommand]
    private void AddSetHighAction(int inputChannel)
    {
        var action = new SetInputHighAction { InputChannel = inputChannel };
        Actions.Add(new SequenceActionViewModel(action));
    }

    /// <summary>
    /// Adds a SetLow action to the sequence.
    /// </summary>
    [RelayCommand]
    private void AddSetLowAction(int inputChannel)
    {
        var action = new SetInputLowAction { InputChannel = inputChannel };
        Actions.Add(new SequenceActionViewModel(action));
    }

    /// <summary>
    /// Adds a Delay action to the sequence.
    /// </summary>
    [RelayCommand]
    private void AddDelayAction(int durationMs)
    {
        var action = new DelayAction { DurationMs = durationMs };
        Actions.Add(new SequenceActionViewModel(action));
    }

    /// <summary>
    /// Removes the selected action from the sequence.
    /// </summary>
    [RelayCommand]
    private void RemoveAction()
    {
        if (SelectedAction != null)
        {
            Actions.Remove(SelectedAction);
            SelectedAction = null;
        }
    }

    /// <summary>
    /// Moves the selected action up in the sequence.
    /// </summary>
    [RelayCommand]
    private void MoveActionUp()
    {
        if (SelectedAction == null)
            return;

        int index = Actions.IndexOf(SelectedAction);
        if (index > 0)
        {
            Actions.Move(index, index - 1);
        }
    }

    /// <summary>
    /// Moves the selected action down in the sequence.
    /// </summary>
    [RelayCommand]
    private void MoveActionDown()
    {
        if (SelectedAction == null)
            return;

        int index = Actions.IndexOf(SelectedAction);
        if (index < Actions.Count - 1)
        {
            Actions.Move(index, index + 1);
        }
    }

    /// <summary>
    /// Executes the current sequence.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExecutePlaySequence))]
    private async Task PlaySequenceAsync()
    {
        if (CurrentProfile == null || Actions.Count == 0)
            return;

        IsExecuting = true;
        _cancellationTokenSource = new CancellationTokenSource();

        try
        {
            int loopCount = IsInfiniteLoop ? int.MaxValue : LoopCount;

            for (int loop = 0; loop < loopCount; loop++)
            {
                if (_cancellationTokenSource.Token.IsCancellationRequested)
                    break;

                foreach (var actionVm in Actions)
                {
                    if (_cancellationTokenSource.Token.IsCancellationRequested)
                        break;

                    // Mark action as executing (visual feedback)
                    await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        actionVm.IsExecuting = true;
                    });

                    CommandLogEntry? logEntry = null;
                    var action = actionVm.Model;

                    // Execute the action
                    switch (action)
                    {
                        case SetInputHighAction highAction:
                            logEntry = await _communicationService.SetInputStateAsync(
                                CurrentProfile,
                                highAction.InputChannel,
                                DigitalInputState.High);

                            // Update UI immediately if successful
                            if (logEntry.Success)
                            {
                                _onInputStateChanged?.Invoke(highAction.InputChannel, DigitalInputState.High);
                            }
                            break;

                        case SetInputLowAction lowAction:
                            logEntry = await _communicationService.SetInputStateAsync(
                                CurrentProfile,
                                lowAction.InputChannel,
                                DigitalInputState.Low);

                            // Update UI immediately if successful
                            if (logEntry.Success)
                            {
                                _onInputStateChanged?.Invoke(lowAction.InputChannel, DigitalInputState.Low);
                            }
                            break;

                        case DelayAction delayAction:
                            await Task.Delay(delayAction.DurationMs, _cancellationTokenSource.Token);
                            logEntry = new CommandLogEntry
                            {
                                Timestamp = DateTime.Now,
                                IpAddress = CurrentProfile.IpAddress,
                                Port = CurrentProfile.Port,
                                Command = "(delay)",
                                Success = true,
                                Description = delayAction.GetDescription()
                            };
                            break;
                    }

                    if (logEntry != null)
                    {
                        _onCommandExecuted(logEntry);
                    }

                    // Brief delay for visual feedback, then clear executing state
                    await Task.Delay(150, _cancellationTokenSource.Token);

                    await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        actionVm.IsExecuting = false;
                    });
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Sequence was stopped - this is expected
        }
        finally
        {
            // Clear all executing states
            foreach (var actionVm in Actions)
            {
                actionVm.IsExecuting = false;
            }

            IsExecuting = false;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }

    /// <summary>
    /// Stops the currently executing sequence.
    /// </summary>
    [RelayCommand]
    private void StopSequence()
    {
        _cancellationTokenSource?.Cancel();
    }

    /// <summary>
    /// Saves the current sequence state.
    /// </summary>
    public TriggerSequence SaveSequence()
    {
        if (CurrentSequence == null)
        {
            CurrentSequence = new TriggerSequence();
        }

        CurrentSequence.Name = SequenceName;
        CurrentSequence.Actions = Actions.Select(a => a.Model).ToList();
        CurrentSequence.LoopCount = IsInfiniteLoop ? 0 : LoopCount;
        CurrentSequence.LastModified = DateTime.Now;

        return CurrentSequence;
    }

    /// <summary>
    /// Updates the current device profile.
    /// </summary>
    public void UpdateProfile(DeviceProfile? profile)
    {
        CurrentProfile = profile;
    }

    /// <summary>
    /// Determines if the play sequence command can execute.
    /// </summary>
    private bool CanExecutePlaySequence() => IsConnected && !IsExecuting && Actions.Count > 0;
}
