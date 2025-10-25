using CommunityToolkit.Mvvm.ComponentModel;
using AdamTriggerSimulator.Models;

namespace AdamTriggerSimulator.ViewModels;

/// <summary>
/// ViewModel wrapper for DeviceProfile with observable properties.
/// </summary>
public partial class DeviceProfileViewModel : ViewModelBase
{
    private readonly DeviceProfile _profile;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayText))]
    private string _name;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayText))]
    private string _ipAddress;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayText))]
    private int _port;

    [ObservableProperty]
    private string _description;

    public Guid Id => _profile.Id;

    public DeviceProfileViewModel(DeviceProfile profile)
    {
        _profile = profile;
        _name = profile.Name;
        _ipAddress = profile.IpAddress;
        _port = profile.Port;
        _description = profile.Description;
    }

    /// <summary>
    /// Creates a new DeviceProfile from the current ViewModel state.
    /// </summary>
    public DeviceProfile ToModel()
    {
        _profile.Name = Name;
        _profile.IpAddress = IpAddress;
        _profile.Port = Port;
        _profile.Description = Description;
        _profile.LastModified = DateTime.Now;

        return _profile;
    }

    /// <summary>
    /// Gets a display string for this profile.
    /// </summary>
    public string DisplayText => $"{Name} ({IpAddress}:{Port})";
}
