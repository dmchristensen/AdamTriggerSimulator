using CommunityToolkit.Mvvm.ComponentModel;
using AdamTriggerSimulator.Models;

namespace AdamTriggerSimulator.ViewModels;

/// <summary>
/// ViewModel wrapper for SequenceAction with observable properties.
/// </summary>
public partial class SequenceActionViewModel : ViewModelBase
{
    private readonly SequenceAction _action;

    [ObservableProperty]
    private string _description;

    [ObservableProperty]
    private string _actionType;

    [ObservableProperty]
    private bool _isExecuting;

    public Guid Id => _action.Id;

    public SequenceAction Model => _action;

    public SequenceActionViewModel(SequenceAction action)
    {
        _action = action;
        _description = action.GetDescription();
        _actionType = action.ActionType;
    }

    /// <summary>
    /// Updates the description if the underlying action changes.
    /// </summary>
    public void RefreshDescription()
    {
        Description = _action.GetDescription();
    }
}
