using System.Collections.ObjectModel;
using System.Timers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AdamTriggerSimulator.Models;
using AdamTriggerSimulator.Services;

namespace AdamTriggerSimulator.ViewModels;

/// <summary>
/// Main window ViewModel that orchestrates all application features.
/// </summary>
public partial class MainWindowViewModel : ViewModelBase, IDisposable
{
    private readonly IAdamCommunicationService _communicationService;
    private readonly IProfileService _profileService;
    private readonly System.Timers.Timer _pollingTimer;

    [ObservableProperty]
    private ObservableCollection<DeviceProfileViewModel> _profiles = new();

    [ObservableProperty]
    private DeviceProfileViewModel? _selectedProfile;

    [ObservableProperty]
    private ObservableCollection<TriggerSequence> _sequences = new();

    [ObservableProperty]
    private TriggerSequence? _selectedSequence;

    [ObservableProperty]
    private ObservableCollection<CommandLogEntry> _commandLog = new();

    [ObservableProperty]
    private ObservableCollection<InputControlViewModel> _inputControls = new();

    [ObservableProperty]
    private SequenceBuilderViewModel _sequenceBuilder;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    // Device information from test connection
    [ObservableProperty]
    private string _deviceVersion = string.Empty;

    [ObservableProperty]
    private string _deviceModel = string.Empty;

    [ObservableProperty]
    private string _deviceName = string.Empty;

    [ObservableProperty]
    private bool _deviceInfoAvailable;

    [ObservableProperty]
    private bool _isTesting;

    [ObservableProperty]
    private bool _testFailed;

    [ObservableProperty]
    private string _failureMessage = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SetAllLowCommand))]
    [NotifyCanExecuteChangedFor(nameof(SetAllHighCommand))]
    [NotifyCanExecuteChangedFor(nameof(StartPollingCommand))]
    [NotifyCanExecuteChangedFor(nameof(TogglePollingCommand))]
    private bool _isConnected;

    // Overlay for modal dialogs
    [ObservableProperty]
    private bool _isOverlayVisible;

    // Flag to indicate if welcome dialog should be shown after window loads
    private bool _shouldShowWelcomeDialog;

    // Status polling fields
    [ObservableProperty]
    private bool _isPolling;

    [ObservableProperty]
    private int _pollingIntervalSeconds = 2;

    private int _consecutivePollingFailures = 0;
    private const int MaxConsecutiveFailures = 3;

    public MainWindowViewModel()
    {
        // Initialize services
        _communicationService = new AdamCommunicationService();
        _profileService = new ProfileService();

        // Initialize sequence builder
        _sequenceBuilder = new SequenceBuilderViewModel(
            _communicationService,
            AddCommandLog,
            UpdateInputState);

        // Initialize input controls (DI0-DI5)
        for (int i = 0; i < 6; i++)
        {
            var inputControl = new InputControlViewModel(i, _communicationService, AddCommandLog);
            InputControls.Add(inputControl);
        }

        // Initialize polling timer
        _pollingTimer = new System.Timers.Timer();
        _pollingTimer.Elapsed += OnPollingTimerElapsed;
        _pollingTimer.AutoReset = true;

        // Load saved data
        _ = InitializeAsync();
    }

    /// <summary>
    /// Initializes the application by loading saved profiles and sequences.
    /// </summary>
    private async Task InitializeAsync()
    {
        try
        {
            // Load profiles
            var profiles = await _profileService.LoadProfilesAsync();
            foreach (var profile in profiles)
            {
                Profiles.Add(new DeviceProfileViewModel(profile));
            }

            // Select first profile by default
            if (Profiles.Count > 0)
            {
                SelectedProfile = Profiles[0];
            }

            // Load sequences
            var sequences = await _profileService.LoadSequencesAsync();
            foreach (var sequence in sequences)
            {
                Sequences.Add(sequence);
            }

            // Select first sequence by default
            if (Sequences.Count > 0)
            {
                SelectedSequence = Sequences[0];
                SequenceBuilder.LoadSequence(SelectedSequence);
            }

            StatusMessage = "Loaded profiles and sequences";

            // Set flag to show welcome dialog after window is shown
            if (Profiles.Count == 0)
            {
                _shouldShowWelcomeDialog = true;
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading data: {ex.Message}";
        }
    }

    /// <summary>
    /// Called after the main window is loaded. Shows welcome dialog if needed.
    /// </summary>
    public void OnWindowLoaded()
    {
        if (_shouldShowWelcomeDialog)
        {
            _shouldShowWelcomeDialog = false;
            ShowWelcomeDialog();
        }
        else if (SelectedProfile != null)
        {
            // Automatically test connection on app load if a profile is selected
            _ = AutoTestConnectionAsync();
        }
    }

    /// <summary>
    /// Shows the welcome dialog and prompts user to create a profile.
    /// </summary>
    private void ShowWelcomeDialog()
    {
        IsOverlayVisible = true;

        var welcomeDialog = new Views.WelcomeDialog
        {
            Owner = System.Windows.Application.Current.MainWindow
        };

        var result = welcomeDialog.ShowDialog();

        IsOverlayVisible = false;

        // If user clicked "Create New Profile", open the profile editor
        if (result == true)
        {
            _ = CreateProfileAsync();
        }
    }

    /// <summary>
    /// Called when profile selection changes.
    /// </summary>
    partial void OnSelectedProfileChanged(DeviceProfileViewModel? value)
    {
        var profile = value?.ToModel();

        // Update all input controls with new profile
        foreach (var inputControl in InputControls)
        {
            inputControl.UpdateProfile(profile);
        }

        // Update sequence builder with new profile
        SequenceBuilder.UpdateProfile(profile);

        // Automatically test connection when a profile is selected
        // Polling will be started/stopped automatically based on connection state
        if (profile != null)
        {
            _ = AutoTestConnectionAsync();
        }
        else
        {
            // No profile selected - mark as disconnected which will stop polling
            IsConnected = false;
        }
    }

    /// <summary>
    /// Called when sequence selection changes.
    /// </summary>
    partial void OnSelectedSequenceChanged(TriggerSequence? value)
    {
        if (value != null)
        {
            SequenceBuilder.LoadSequence(value);
        }
    }

    /// <summary>
    /// Adds a command log entry to the log.
    /// </summary>
    private void AddCommandLog(CommandLogEntry entry)
    {
        CommandLog.Insert(0, entry); // Insert at top for newest first

        // Limit log to 100 entries
        while (CommandLog.Count > 100)
        {
            CommandLog.RemoveAt(CommandLog.Count - 1);
        }
    }

    /// <summary>
    /// Updates the visual state of an input control immediately after a command.
    /// </summary>
    private void UpdateInputState(int inputChannel, DigitalInputState state)
    {
        if (inputChannel >= 0 && inputChannel < InputControls.Count)
        {
            InputControls[inputChannel].UpdateState(state);
        }
    }

    /// <summary>
    /// Clears the command log.
    /// </summary>
    [RelayCommand]
    private void ClearLog()
    {
        CommandLog.Clear();
        StatusMessage = "Log cleared";
    }

    /// <summary>
    /// Opens the Profile Editor dialog to create a new profile.
    /// </summary>
    [RelayCommand]
    private async Task CreateProfileAsync()
    {
        IsOverlayVisible = true;

        var dialog = new Views.ProfileEditorDialog();
        dialog.ViewModel.InitializeForCreate();
        dialog.Owner = System.Windows.Application.Current.MainWindow;

        var result = dialog.ShowDialog();

        IsOverlayVisible = false;

        if (result == true)
        {
            var profile = dialog.ViewModel.GetProfile();
            var profileVm = new DeviceProfileViewModel(profile);
            Profiles.Add(profileVm);
            SelectedProfile = profileVm;

            await SaveProfilesAsync();
            StatusMessage = $"Profile '{profile.Name}' created";
        }
    }

    /// <summary>
    /// Opens the Profile Editor dialog to edit the selected profile.
    /// </summary>
    [RelayCommand]
    private async Task EditProfileAsync()
    {
        if (SelectedProfile == null)
            return;

        IsOverlayVisible = true;

        var dialog = new Views.ProfileEditorDialog();
        dialog.ViewModel.InitializeForEdit(SelectedProfile.ToModel());
        dialog.Owner = System.Windows.Application.Current.MainWindow;

        var result = dialog.ShowDialog();

        IsOverlayVisible = false;

        if (result == true)
        {
            var updatedProfile = dialog.ViewModel.GetProfile();
            SelectedProfile.Name = updatedProfile.Name;
            SelectedProfile.IpAddress = updatedProfile.IpAddress;
            SelectedProfile.Port = updatedProfile.Port;
            SelectedProfile.Description = updatedProfile.Description;

            await SaveProfilesAsync();
            StatusMessage = $"Profile '{updatedProfile.Name}' updated";

            // Automatically test connection after profile edit
            _ = AutoTestConnectionAsync();
        }
    }

    /// <summary>
    /// Deletes the selected profile.
    /// </summary>
    [RelayCommand]
    private async Task DeleteProfileAsync()
    {
        if (SelectedProfile == null)
            return;

        var profileName = SelectedProfile.Name;

        IsOverlayVisible = true;

        // Confirm deletion
        var confirmed = Views.ConfirmationDialog.Show(
            "Confirm Delete",
            $"Are you sure you want to delete the profile '{profileName}'?",
            System.Windows.Application.Current.MainWindow);

        IsOverlayVisible = false;

        if (!confirmed)
            return;

        Profiles.Remove(SelectedProfile);

        // Select first available profile
        SelectedProfile = Profiles.FirstOrDefault();

        await SaveProfilesAsync();

        StatusMessage = $"Profile '{profileName}' deleted";

        // Show welcome dialog if no profiles remain
        if (Profiles.Count == 0)
        {
            ShowWelcomeDialog();
        }
    }

    /// <summary>
    /// Tests connection to the selected profile and retrieves device information.
    /// </summary>
    [RelayCommand]
    private async Task TestConnectionAsync()
    {
        await AutoTestConnectionAsync();
    }

    /// <summary>
    /// Automatically tests connection and updates UI state.
    /// </summary>
    private async Task AutoTestConnectionAsync()
    {
        if (SelectedProfile == null)
            return;

        // Reset states
        IsTesting = true;
        DeviceInfoAvailable = false;
        TestFailed = false;
        FailureMessage = string.Empty;
        StatusMessage = "Testing connection...";

        var profile = SelectedProfile.ToModel();
        var logEntry = await _communicationService.TestConnectionAsync(profile);
        AddCommandLog(logEntry);

        IsTesting = false;

        if (logEntry.Success && !string.IsNullOrEmpty(logEntry.Response))
        {
            // Parse the response which contains: "Version: {version} | Model: {model} | Name: {name}"
            var parts = logEntry.Response.Split('|');
            if (parts.Length >= 3)
            {
                DeviceVersion = parts[0].Replace("Version:", "").Trim();
                DeviceModel = parts[1].Replace("Model:", "").Trim();
                DeviceName = parts[2].Replace("Name:", "").Trim();
                DeviceInfoAvailable = true;
                TestFailed = false;
                IsConnected = true;
            }

            StatusMessage = "Connection successful - Device info retrieved";
        }
        else
        {
            DeviceVersion = string.Empty;
            DeviceModel = string.Empty;
            DeviceName = string.Empty;
            DeviceInfoAvailable = false;
            TestFailed = true;
            IsConnected = false;

            // Set user-friendly failure message
            if (!string.IsNullOrEmpty(logEntry.ErrorMessage))
            {
                if (logEntry.ErrorMessage.Contains("timeout", StringComparison.OrdinalIgnoreCase))
                {
                    FailureMessage = "We couldn't reach your device. Please make sure it's powered on and connected to the network.";
                }
                else if (logEntry.ErrorMessage.Contains("connection", StringComparison.OrdinalIgnoreCase))
                {
                    FailureMessage = "Unable to establish a connection. Please verify the IP address and port are correct.";
                }
                else
                {
                    FailureMessage = "Something went wrong while trying to connect. Please check your device settings and try again.";
                }
            }
            else
            {
                FailureMessage = "Unable to connect to the device. Please check the connection settings and try again.";
            }

            StatusMessage = "Connection failed";
        }
    }

    /// <summary>
    /// Opens the Manual Command dialog.
    /// </summary>
    [RelayCommand]
    private void OpenManualCommandDialog()
    {
        if (SelectedProfile == null)
        {
            StatusMessage = "Please select a profile first";
            return;
        }

        IsOverlayVisible = true;

        var dialog = new Views.ManualCommandDialog(_communicationService, SelectedProfile.ToModel());
        dialog.Owner = System.Windows.Application.Current.MainWindow;
        dialog.ShowDialog();

        IsOverlayVisible = false;
    }

    /// <summary>
    /// Sets all digital outputs to LOW.
    /// </summary>
    [RelayCommand(CanExecute = nameof(IsConnected))]
    private async Task SetAllLowAsync()
    {
        if (SelectedProfile == null)
            return;

        StatusMessage = "Setting all outputs to LOW...";
        var profile = SelectedProfile.ToModel();
        var logEntry = await _communicationService.SetAllLowAsync(profile);
        AddCommandLog(logEntry);

        // Update all input control states to LOW
        foreach (var inputControl in InputControls)
        {
            inputControl.UpdateState(DigitalInputState.Low);
        }

        StatusMessage = logEntry.Success ? "All outputs set to LOW" : "Failed to set outputs";
    }

    /// <summary>
    /// Sets all digital outputs to HIGH.
    /// </summary>
    [RelayCommand(CanExecute = nameof(IsConnected))]
    private async Task SetAllHighAsync()
    {
        if (SelectedProfile == null)
            return;

        StatusMessage = "Setting all outputs to HIGH...";
        var profile = SelectedProfile.ToModel();
        var logEntry = await _communicationService.SetAllHighAsync(profile);
        AddCommandLog(logEntry);

        // Update all input control states to HIGH
        foreach (var inputControl in InputControls)
        {
            inputControl.UpdateState(DigitalInputState.High);
        }

        StatusMessage = logEntry.Success ? "All outputs set to HIGH" : "Failed to set outputs";
    }

    /// <summary>
    /// Reads the current status of all digital outputs from the device.
    /// </summary>
    [RelayCommand]
    private async Task ReadStatusAsync()
    {
        if (SelectedProfile == null)
            return;

        StatusMessage = "Reading current status...";
        var profile = SelectedProfile.ToModel();
        var logEntry = await _communicationService.GetCurrentStatusAsync(profile);
        AddCommandLog(logEntry);

        if (logEntry.Success)
        {
            // Get the current states from the service and update UI
            var currentStates = _communicationService.GetCurrentStates();
            foreach (var kvp in currentStates)
            {
                if (kvp.Key < InputControls.Count)
                {
                    InputControls[kvp.Key].UpdateState(kvp.Value);
                }
            }

            StatusMessage = "Status read successfully";
        }
        else
        {
            StatusMessage = "Failed to read status";
        }
    }

    /// <summary>
    /// Opens the Sequence Editor dialog to create a new sequence.
    /// </summary>
    [RelayCommand]
    private async Task CreateSequenceAsync()
    {
        IsOverlayVisible = true;

        var dialog = new Views.SequenceEditorDialog();
        dialog.ViewModel.InitializeForCreate();
        dialog.Owner = System.Windows.Application.Current.MainWindow;

        var result = dialog.ShowDialog();

        IsOverlayVisible = false;

        if (result == true)
        {
            var sequence = dialog.ViewModel.GetSequence();
            Sequences.Add(sequence);
            SelectedSequence = sequence;

            // Load the sequence into the builder for playback
            SequenceBuilder.LoadSequence(sequence);

            await SaveSequencesAsync();
            StatusMessage = $"Sequence '{sequence.Name}' created";
        }
    }

    /// <summary>
    /// Opens the Sequence Editor dialog to edit the selected sequence.
    /// </summary>
    [RelayCommand]
    private async Task EditSequenceAsync()
    {
        if (SelectedSequence == null)
            return;

        IsOverlayVisible = true;

        var dialog = new Views.SequenceEditorDialog();
        dialog.ViewModel.InitializeForEdit(SelectedSequence);
        dialog.Owner = System.Windows.Application.Current.MainWindow;

        var result = dialog.ShowDialog();

        IsOverlayVisible = false;

        if (result == true)
        {
            var updatedSequence = dialog.ViewModel.GetSequence();

            // Update the sequence in the collection
            var index = Sequences.IndexOf(SelectedSequence);
            if (index >= 0)
            {
                Sequences[index] = updatedSequence;
                SelectedSequence = updatedSequence;
            }

            // Reload the sequence into the builder
            SequenceBuilder.LoadSequence(updatedSequence);

            await SaveSequencesAsync();
            StatusMessage = $"Sequence '{updatedSequence.Name}' updated";
        }
    }

    /// <summary>
    /// Deletes the selected sequence.
    /// </summary>
    [RelayCommand]
    private async Task DeleteSequenceAsync()
    {
        if (SelectedSequence == null)
            return;

        var sequenceName = SelectedSequence.Name;

        IsOverlayVisible = true;

        // Confirm deletion
        var confirmed = Views.ConfirmationDialog.Show(
            "Confirm Delete",
            $"Are you sure you want to delete the sequence '{sequenceName}'?",
            System.Windows.Application.Current.MainWindow);

        IsOverlayVisible = false;

        if (!confirmed)
            return;

        Sequences.Remove(SelectedSequence);
        SelectedSequence = Sequences.FirstOrDefault();

        // Load the new selected sequence into builder, or clear it
        if (SelectedSequence != null)
        {
            SequenceBuilder.LoadSequence(SelectedSequence);
        }
        else
        {
            SequenceBuilder.NewSequence();
        }

        await SaveSequencesAsync();
        StatusMessage = $"Sequence '{sequenceName}' deleted";
    }

    /// <summary>
    /// Saves all profiles to disk.
    /// </summary>
    private async Task SaveProfilesAsync()
    {
        var profiles = Profiles.Select(p => p.ToModel()).ToList();
        await _profileService.SaveProfilesAsync(profiles);
    }

    /// <summary>
    /// Saves all sequences to disk.
    /// </summary>
    private async Task SaveSequencesAsync()
    {
        await _profileService.SaveSequencesAsync(Sequences.ToList());
    }

    /// <summary>
    /// Starts status polling.
    /// </summary>
    [RelayCommand(CanExecute = nameof(IsConnected))]
    private void StartPolling()
    {
        if (IsPolling || SelectedProfile == null)
            return;

        _pollingTimer.Interval = PollingIntervalSeconds * 1000; // Convert seconds to milliseconds
        _pollingTimer.Start();
        IsPolling = true;
        StatusMessage = "Status polling started";
    }

    /// <summary>
    /// Stops status polling.
    /// </summary>
    [RelayCommand]
    private void StopPolling()
    {
        if (!IsPolling)
            return;

        _pollingTimer.Stop();
        IsPolling = false;
        StatusMessage = "Status polling stopped";
    }

    /// <summary>
    /// Toggles status polling on/off.
    /// </summary>
    [RelayCommand(CanExecute = nameof(IsConnected))]
    private void TogglePolling()
    {
        if (IsPolling)
            StopPolling();
        else
            StartPolling();
    }

    /// <summary>
    /// Called when the polling timer elapses.
    /// </summary>
    private async void OnPollingTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        if (SelectedProfile == null)
            return;

        try
        {
            var profile = SelectedProfile.ToModel();
            var logEntry = await _communicationService.GetCurrentStatusAsync(profile);

            if (logEntry.Success)
            {
                // Reset failure counter on success
                _consecutivePollingFailures = 0;

                // Get the current states from the service and update UI
                var currentStates = _communicationService.GetCurrentStates();

                // Update UI on the UI thread
                System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                {
                    foreach (var kvp in currentStates)
                    {
                        if (kvp.Key < InputControls.Count)
                        {
                            InputControls[kvp.Key].UpdateState(kvp.Value);
                        }
                    }
                });
            }
            else
            {
                // Command failed - increment failure counter
                _consecutivePollingFailures++;

                if (_consecutivePollingFailures >= MaxConsecutiveFailures)
                {
                    // Too many failures - mark as disconnected and stop polling
                    System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                    {
                        IsConnected = false;
                        StatusMessage = $"Connection lost - polling stopped after {MaxConsecutiveFailures} failures";
                    });
                }
            }
        }
        catch (Exception ex)
        {
            // Increment failure counter on exception
            _consecutivePollingFailures++;

            // Log error for debugging
            System.Diagnostics.Debug.WriteLine($"Polling error: {ex.Message}");

            // Stop polling after too many consecutive failures to prevent error spam
            if (_consecutivePollingFailures >= MaxConsecutiveFailures)
            {
                System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                {
                    IsConnected = false;
                    StatusMessage = $"Connection lost - polling stopped after {MaxConsecutiveFailures} failures";
                });
            }
        }
    }

    /// <summary>
    /// Called when IsConnected property changes. Updates all input controls and manages polling.
    /// </summary>
    partial void OnIsConnectedChanged(bool value)
    {
        foreach (var inputControl in InputControls)
        {
            inputControl.IsConnected = value;
        }

        // Also update sequence builder
        SequenceBuilder.IsConnected = value;

        // Automatically manage polling based on connection state
        if (value)
        {
            // Connection established - reset failure counter and start polling automatically
            _consecutivePollingFailures = 0;
            StartPolling();
        }
        else
        {
            // Connection lost - stop polling
            StopPolling();
        }
    }

    /// <summary>
    /// Disposes resources.
    /// </summary>
    public void Dispose()
    {
        _pollingTimer?.Stop();
        _pollingTimer?.Dispose();
    }
}
