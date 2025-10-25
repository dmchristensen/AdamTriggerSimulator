using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AdamTriggerSimulator.Models;

namespace AdamTriggerSimulator.ViewModels;

/// <summary>
/// ViewModel for the Sequence Editor dialog (used for both creating and editing sequences).
/// </summary>
public partial class SequenceEditorDialogViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _sequenceName = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private ObservableCollection<SequenceActionViewModel> _actions = new();

    [ObservableProperty]
    private SequenceActionViewModel? _selectedAction;

    [ObservableProperty]
    private int _loopCount = 1;

    [ObservableProperty]
    private bool _isInfiniteLoop;

    [ObservableProperty]
    private string _dialogTitle = "New Sequence";

    [ObservableProperty]
    private bool _isEditMode;

    [ObservableProperty]
    private string _validationError = string.Empty;

    private TriggerSequence? _currentSequence;

    public bool DialogResult { get; private set; }

    public SequenceEditorDialogViewModel()
    {
    }

    /// <summary>
    /// Initializes the dialog for creating a new sequence.
    /// </summary>
    public void InitializeForCreate()
    {
        DialogTitle = "Create New Sequence";
        IsEditMode = false;
        SequenceName = string.Empty;
        Description = string.Empty;
        LoopCount = 1;
        IsInfiniteLoop = false;
        Actions.Clear();
        _currentSequence = null;
    }

    /// <summary>
    /// Initializes the dialog for editing an existing sequence.
    /// </summary>
    public void InitializeForEdit(TriggerSequence sequence)
    {
        DialogTitle = "Edit Sequence";
        IsEditMode = true;
        SequenceName = sequence.Name;
        Description = sequence.Description;
        LoopCount = sequence.LoopCount == 0 ? 1 : sequence.LoopCount;
        IsInfiniteLoop = sequence.LoopCount == 0;

        Actions.Clear();
        foreach (var action in sequence.Actions)
        {
            Actions.Add(new SequenceActionViewModel(action));
        }

        _currentSequence = sequence;
    }

    /// <summary>
    /// Creates a TriggerSequence from the current values.
    /// </summary>
    public TriggerSequence GetSequence()
    {
        // Always create a new object to ensure ObservableCollection detects the change
        var sequence = new TriggerSequence
        {
            Id = _currentSequence?.Id ?? Guid.NewGuid(),
            CreatedAt = _currentSequence?.CreatedAt ?? DateTime.Now,
            Name = SequenceName,
            Description = Description,
            Actions = Actions.Select(a => a.Model).ToList(),
            LoopCount = IsInfiniteLoop ? 0 : LoopCount,
            LastModified = DateTime.Now
        };

        return sequence;
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
    /// Removes the specified action from the sequence.
    /// </summary>
    [RelayCommand]
    private void RemoveAction(SequenceActionViewModel? action)
    {
        if (action != null && Actions.Contains(action))
        {
            Actions.Remove(action);
            if (SelectedAction == action)
            {
                SelectedAction = null;
            }
        }
    }

    /// <summary>
    /// Moves the specified action up in the sequence.
    /// </summary>
    [RelayCommand]
    private void MoveActionUp(SequenceActionViewModel? action)
    {
        if (action == null || !Actions.Contains(action))
            return;

        int index = Actions.IndexOf(action);
        if (index > 0)
        {
            Actions.Move(index, index - 1);
        }
    }

    /// <summary>
    /// Moves the specified action down in the sequence.
    /// </summary>
    [RelayCommand]
    private void MoveActionDown(SequenceActionViewModel? action)
    {
        if (action == null || !Actions.Contains(action))
            return;

        int index = Actions.IndexOf(action);
        if (index < Actions.Count - 1)
        {
            Actions.Move(index, index + 1);
        }
    }

    /// <summary>
    /// Saves the sequence and closes the dialog.
    /// </summary>
    [RelayCommand]
    private void Save()
    {
        ValidationError = string.Empty;

        if (string.IsNullOrWhiteSpace(SequenceName))
        {
            ValidationError = "Sequence name is required.";
            return;
        }

        if (Actions.Count == 0)
        {
            ValidationError = "At least one action is required.";
            return;
        }

        DialogResult = true;
    }

    /// <summary>
    /// Cancels and closes the dialog.
    /// </summary>
    [RelayCommand]
    private void Cancel()
    {
        DialogResult = false;
    }
}
