using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AdamTriggerSimulator.Models;

namespace AdamTriggerSimulator.ViewModels;

/// <summary>
/// ViewModel for the Profile Editor dialog (used for both creating and editing profiles).
/// </summary>
public partial class ProfileEditorDialogViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _profileName = string.Empty;

    [ObservableProperty]
    private string _ipAddress = "192.168.0.31";

    [ObservableProperty]
    private int _port = 1025;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _dialogTitle = "New Profile";

    [ObservableProperty]
    private bool _isEditMode;

    public bool DialogResult { get; private set; }

    public ProfileEditorDialogViewModel()
    {
    }

    /// <summary>
    /// Initializes the dialog for creating a new profile.
    /// </summary>
    public void InitializeForCreate()
    {
        DialogTitle = "Create New Profile";
        IsEditMode = false;
        ProfileName = string.Empty;
        IpAddress = "192.168.0.31";
        Port = 1025;
        Description = string.Empty;
    }

    /// <summary>
    /// Initializes the dialog for editing an existing profile.
    /// </summary>
    public void InitializeForEdit(DeviceProfile profile)
    {
        DialogTitle = "Edit Profile";
        IsEditMode = true;
        ProfileName = profile.Name;
        IpAddress = profile.IpAddress;
        Port = profile.Port;
        Description = profile.Description;
    }

    /// <summary>
    /// Creates a DeviceProfile from the current values.
    /// </summary>
    public DeviceProfile GetProfile()
    {
        return new DeviceProfile
        {
            Name = ProfileName,
            IpAddress = IpAddress,
            Port = Port,
            Description = Description
        };
    }

    /// <summary>
    /// Saves the profile and closes the dialog.
    /// </summary>
    [RelayCommand]
    private void Save()
    {
        if (string.IsNullOrWhiteSpace(ProfileName))
        {
            // Validation failed
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
