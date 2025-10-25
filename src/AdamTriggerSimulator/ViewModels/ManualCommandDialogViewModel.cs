using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AdamTriggerSimulator.Models;
using AdamTriggerSimulator.Services;

namespace AdamTriggerSimulator.ViewModels;

/// <summary>
/// ViewModel for the Manual Command dialog.
/// </summary>
public partial class ManualCommandDialogViewModel : ViewModelBase
{
    private readonly IAdamCommunicationService _communicationService;
    private DeviceProfile? _profile;

    [ObservableProperty]
    private string _command = string.Empty;

    [ObservableProperty]
    private string _response = "(no response yet)";

    [ObservableProperty]
    private bool _isSending;

    public ManualCommandDialogViewModel(IAdamCommunicationService communicationService)
    {
        _communicationService = communicationService;
    }

    /// <summary>
    /// Sets the device profile to send commands to.
    /// </summary>
    public void SetProfile(DeviceProfile profile)
    {
        _profile = profile;
    }

    /// <summary>
    /// Sends the manual UDP command.
    /// </summary>
    [RelayCommand]
    private async Task SendCommandAsync()
    {
        if (_profile == null || string.IsNullOrWhiteSpace(Command))
            return;

        IsSending = true;
        Response = "Sending...";

        try
        {
            // Add \r terminator if not present
            string command = Command;
            if (!command.EndsWith("\r"))
            {
                command += "\r";
            }

            var logEntry = await _communicationService.SendCommandAsync(_profile, command);
            Response = logEntry.Success
                ? (logEntry.Response ?? "(no response)")
                : $"Error: {logEntry.ErrorMessage}";
        }
        catch (Exception ex)
        {
            Response = $"Exception: {ex.Message}";
        }
        finally
        {
            IsSending = false;
        }
    }

    /// <summary>
    /// Clears the command and response fields.
    /// </summary>
    [RelayCommand]
    private void Clear()
    {
        Command = string.Empty;
        Response = "(no response yet)";
    }
}
